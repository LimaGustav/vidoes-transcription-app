using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using VideoProcessorWorker.Infra.Persistence.Contexts;
using VideoProcessorWorker.Infra.Storage;
using VideoProcessorWorker.Services.Audio;
using VideoProcessorWorker.Services.Transcription;

namespace VideoProcessorWorker.Application.GenerateSubtitles
{
    public class GenerateSubtitlesUseCase : IGenerateSubtitlesUseCase
    {
        private readonly VideoDbContext _dbContext;
        private readonly IObjectStorageService _storageService;
        private readonly IAudioExtractionService _audioExtractionService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly ILogger<GenerateSubtitlesUseCase> _logger;

        public GenerateSubtitlesUseCase(
            VideoDbContext dbContext,
            IObjectStorageService storageService,
            IAudioExtractionService audioExtractionService,
            ITranscriptionService transcriptionService,
            ILogger<GenerateSubtitlesUseCase> logger)
        {
            _dbContext = dbContext;
            _storageService = storageService;
            _audioExtractionService = audioExtractionService;
            _transcriptionService = transcriptionService;
            _logger = logger;
        }

        public async Task<bool> ExecuteAsync(MinioNotificationEvent notification, CancellationToken cancellationToken = default)
        {
            if (notification.Records == null || notification.Records.Count == 0)
            {
                _logger.LogWarning("Evento do MinIO recebido sem registros.");
                return false;
            }

            var record = notification.Records[0];
            var eventName = record.EventName ?? notification.EventName ?? string.Empty;

            // Filtra apenas eventos de criação de objeto
            if (!eventName.StartsWith("s3:ObjectCreated:", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Ignorando evento não relacionado à criação de objeto: {EventName}", eventName);
                return true;
            }

            var bucket = record.S3?.Bucket?.Name;
            var rawObjectKey = record.S3?.Object?.Key;

            if (string.IsNullOrWhiteSpace(bucket) || string.IsNullOrWhiteSpace(rawObjectKey))
            {
                _logger.LogWarning("Bucket ou chave do objeto não informados na mensagem do MinIO.");
                return false;
            }

            var decodedObjectKey = record.S3!.Object!.GetDecodedKey();
            var requestId = record.GetRequestId() ?? Guid.NewGuid().ToString("N");

            _logger.LogInformation("Processando evento MinIO para Bucket: {Bucket}, Key: {Key}, RequestId: {RequestId}",
                bucket, decodedObjectKey, requestId);

            // 1. Busca o vídeo correspondente no banco de dados
            var video = await _dbContext.Videos
                .FirstOrDefaultAsync(v => v.MinioBucket == bucket && v.MinioObjectKey == decodedObjectKey, cancellationToken);

            if (video == null)
            {
                // Fallback: tentar localizar por GUID no caminho do objeto
                var segments = decodedObjectKey.Split('/', StringSplitOptions.RemoveEmptyEntries);
                foreach (var seg in segments)
                {
                    if (Guid.TryParse(seg, out var videoGuid))
                    {
                        video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == videoGuid, cancellationToken);
                        if (video != null) break;
                    }
                }
            }

            if (video == null)
            {
                _logger.LogWarning("Nenhum registro de vídeo encontrado no banco para Bucket: {Bucket}, Key: {Key}. Ignorando.",
                    bucket, decodedObjectKey);
                return true;
            }

            // 2. Idempotência e alteração de status para "Processing"
            if (!string.IsNullOrEmpty(video.IdpotencyKey) && video.IdpotencyKey == requestId)
            {
                _logger.LogWarning("Vídeo {VideoId} já foi/está sendo processado com IdpotencyKey {RequestId}.", video.Id, requestId);
                return true;
            }

            // Tenta adquirir o processamento de forma atômica
            try
            {
                var rowsAffected = await _dbContext.Videos
                    .Where(v => v.Id == video.Id && (v.IdpotencyKey == null || v.IdpotencyKey != requestId))
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(v => v.IdpotencyKey, requestId)
                        .SetProperty(v => v.Status, "Processing")
                        .SetProperty(v => v.UpdatedAt, DateTime.UtcNow), cancellationToken);

                if (rowsAffected == 0)
                {
                    _logger.LogWarning("Falha ao atualizar idempotência para o vídeo {VideoId}. Outro worker pode ter assumido o processamento.", video.Id);
                    return true;
                }

                // Atualiza o estado da entidade local
                video.IdpotencyKey = requestId;
                video.Status = "Processing";
                video.UpdatedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de concorrência/idempotência ao tentar atualizar status para Processing no vídeo {VideoId}.", video.Id);
                return false;
            }

            string? tempVideoPath = null;
            string? tempAudioPath = null;

            try
            {
                var tempFolder = Path.Combine(Path.GetTempPath(), "VideoProcessor", video.Id.ToString());
                Directory.CreateDirectory(tempFolder);

                tempVideoPath = Path.Combine(tempFolder, $"video_{Guid.NewGuid():N}.mp4");
                tempAudioPath = Path.Combine(tempFolder, $"audio_{Guid.NewGuid():N}.wav");

                // 3. Download do vídeo original do MinIO
                _logger.LogInformation("Baixando vídeo {VideoId} do MinIO ({Bucket}/{Key}) para {TempPath}...",
                    video.Id, bucket, decodedObjectKey, tempVideoPath);
                await _storageService.DownloadObjectToFileAsync(bucket, decodedObjectKey, tempVideoPath, cancellationToken);

                // 3.1 Extração de áudio com FFmpeg
                _logger.LogInformation("Extraindo áudio do vídeo {VideoId}...", video.Id);
                await _audioExtractionService.ExtractAudioForWhisperAsync(tempVideoPath, tempAudioPath, cancellationToken);

                // Remove o vídeo baixado imediatamente para liberar espaço
                if (File.Exists(tempVideoPath))
                {
                    File.Delete(tempVideoPath);
                    tempVideoPath = null;
                }

                // 4. Extração da transcrição do áudio com Whisper (medium + detecção de idioma)
                _logger.LogInformation("Executando transcrição Whisper para o vídeo {VideoId}...", video.Id);
                var transcriptionResult = await _transcriptionService.TranscribeAsync(tempAudioPath, language: "auto", cancellationToken);

                // 5. Excluir o áudio temporário após a transcrição
                if (File.Exists(tempAudioPath))
                {
                    File.Delete(tempAudioPath);
                    tempAudioPath = null;
                }

                // 6. Salvar transcrição e legenda no MinIO em pasta diferente com o mesmo ID
                // Estrutura: subtitles/{videoId}/subtitles.vtt, subtitles.srt, transcript.txt
                var subtitlePrefix = $"subtitles/{video.Id}";

                // Upload do VTT
                var vttBytes = Encoding.UTF8.GetBytes(transcriptionResult.VttContent);
                using (var vttStream = new MemoryStream(vttBytes))
                {
                    await _storageService.UploadStreamAsync(bucket, $"{subtitlePrefix}/subtitles.vtt", vttStream, "text/vtt; charset=utf-8", cancellationToken);
                }

                // Upload do SRT
                var srtBytes = Encoding.UTF8.GetBytes(transcriptionResult.SrtContent);
                using (var srtStream = new MemoryStream(srtBytes))
                {
                    await _storageService.UploadStreamAsync(bucket, $"{subtitlePrefix}/subtitles.srt", srtStream, "text/plain; charset=utf-8", cancellationToken);
                }

                // Upload do texto completo da transcrição
                var txtBytes = Encoding.UTF8.GetBytes(transcriptionResult.FullText);
                using (var txtStream = new MemoryStream(txtBytes))
                {
                    await _storageService.UploadStreamAsync(bucket, $"{subtitlePrefix}/transcript.txt", txtStream, "text/plain; charset=utf-8", cancellationToken);
                }

                _logger.LogInformation("Legendas e transcrição salvas no MinIO com sucesso para o vídeo {VideoId}.", video.Id);

                // 7. Atualiza o status do vídeo para "Ready"
                await _dbContext.Videos
                    .Where(v => v.Id == video.Id)
                    .ExecuteUpdateAsync(setter => setter
                        .SetProperty(v => v.Status, "Ready")
                        .SetProperty(v => v.UpdatedAt, DateTime.UtcNow), cancellationToken);

                _logger.LogInformation("Processamento de legendas concluído com sucesso para o vídeo {VideoId}.", video.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processamento de legendas para o vídeo {VideoId}.", video.Id);

                try
                {
                    await _dbContext.Videos
                        .Where(v => v.Id == video.Id)
                        .ExecuteUpdateAsync(setter => setter
                            .SetProperty(v => v.Status, "Failed")
                            .SetProperty(v => v.UpdatedAt, DateTime.UtcNow), CancellationToken.None);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Falha ao atualizar status para Failed para o vídeo {VideoId}.", video.Id);
                }

                throw;
            }
            finally
            {
                // Limpeza garantida de arquivos temporários
                try
                {
                    if (!string.IsNullOrEmpty(tempVideoPath) && File.Exists(tempVideoPath))
                    {
                        File.Delete(tempVideoPath);
                    }

                    if (!string.IsNullOrEmpty(tempAudioPath) && File.Exists(tempAudioPath))
                    {
                        File.Delete(tempAudioPath);
                    }

                    var tempFolder = Path.Combine(Path.GetTempPath(), "VideoProcessor", video.Id.ToString());
                    if (Directory.Exists(tempFolder) && !Directory.EnumerateFileSystemEntries(tempFolder).Any())
                    {
                        Directory.Delete(tempFolder, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Não foi possível limpar completamente os arquivos temporários do vídeo {VideoId}.", video.Id);
                }
            }
        }
    }
}
