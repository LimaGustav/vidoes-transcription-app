using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Whisper.net;
using Whisper.net.Ggml;

namespace VideoProcessorWorker.Services.Transcription
{
    public class WhisperTranscriptionService : ITranscriptionService, IAsyncDisposable, IDisposable
    {
        private readonly WhisperSettings _settings;
        private readonly ILogger<WhisperTranscriptionService> _logger;
        private WhisperFactory? _factory;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public WhisperTranscriptionService(IOptions<WhisperSettings> options, ILogger<WhisperTranscriptionService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        private async Task<WhisperFactory> GetOrCreateFactoryAsync(CancellationToken cancellationToken)
        {
            if (_factory != null)
            {
                return _factory;
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_factory != null)
                {
                    return _factory;
                }

                var modelPath = _settings.ModelPath;

                // Se o modelo não existir localmente, realiza o download automático do modelo GGML
                if (!File.Exists(modelPath))
                {
                    _logger.LogInformation("Modelo Whisper '{ModelPath}' não encontrado. Iniciando download do modelo {Type}...", 
                        modelPath, _settings.AutoDownloadModelType);

                    var modelType = _settings.AutoDownloadModelType.ToLowerInvariant() switch
                    {
                        "tiny" => GgmlType.Tiny,
                        "small" => GgmlType.Small,
                        "medium" => GgmlType.Medium,
                        "large" => GgmlType.LargeV3,
                        _ => GgmlType.Base
                    };

                    using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(modelType, cancellationToken: cancellationToken);
                    using var fileStream = File.OpenWrite(modelPath);
                    await modelStream.CopyToAsync(fileStream, cancellationToken);

                    _logger.LogInformation("Download do modelo Whisper concluído em: {ModelPath}", modelPath);
                }

                _logger.LogInformation("Carregando WhisperFactory a partir de {ModelPath} (GPU: {UseGpu})", modelPath, _settings.UseGpu);
                _factory = WhisperFactory.FromPath(modelPath, new WhisperFactoryOptions
                {
                    UseGpu = _settings.UseGpu
                });

                return _factory;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<TranscriptionResult> TranscribeAsync(string audioWavPath, string language = "auto", CancellationToken cancellationToken = default)
        {
            if (!File.Exists(audioWavPath))
            {
                throw new FileNotFoundException($"Arquivo de áudio não encontrado: {audioWavPath}");
            }

            using var fileStream = File.OpenRead(audioWavPath);
            return await TranscribeAsync(fileStream, language, cancellationToken);
        }

        public async Task<TranscriptionResult> TranscribeAsync(Stream audioStream, string language = "auto", CancellationToken cancellationToken = default)
        {
            var factory = await GetOrCreateFactoryAsync(cancellationToken);

            var targetLanguage = string.IsNullOrWhiteSpace(language) || language.Equals("auto", StringComparison.OrdinalIgnoreCase)
                ? _settings.DefaultLanguage
                : language;

            var builder = factory.CreateBuilder();

            if (targetLanguage.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                builder.WithLanguageDetection();
            }
            else
            {
                builder.WithLanguage(targetLanguage);
            }

            using var processor = builder.Build();

            var segments = new List<SubtitleSegment>();
            var fullTextBuilder = new StringBuilder();

            _logger.LogInformation("Iniciando transcrição de áudio com Whisper...");

            await foreach (var result in processor.ProcessAsync(audioStream, cancellationToken))
            {
                var text = result.Text.Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                segments.Add(new SubtitleSegment
                {
                    Start = result.Start,
                    End = result.End,
                    Text = text
                });

                fullTextBuilder.Append(text).Append(' ');
            }

            _logger.LogInformation("Transcrição concluída com sucesso. Total de segmentos: {Count}", segments.Count);

            var transcriptionResult = new TranscriptionResult
            {
                Language = targetLanguage,
                FullText = fullTextBuilder.ToString().Trim(),
                Segments = segments,
                VttContent = GenerateWebVtt(segments),
                SrtContent = GenerateSrt(segments)
            };

            return transcriptionResult;
        }

        private static string GenerateWebVtt(List<SubtitleSegment> segments)
        {
            var sb = new StringBuilder();
            sb.AppendLine("WEBVTT");
            sb.AppendLine();

            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                sb.AppendLine($"{i + 1}");
                sb.AppendLine($"{FormatTimeVtt(seg.Start)} --> {FormatTimeVtt(seg.End)}");
                sb.AppendLine(seg.Text);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GenerateSrt(List<SubtitleSegment> segments)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                sb.AppendLine($"{i + 1}");
                sb.AppendLine($"{FormatTimeSrt(seg.Start)} --> {FormatTimeSrt(seg.End)}");
                sb.AppendLine(seg.Text);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string FormatTimeVtt(TimeSpan ts) =>
            $"{Math.Floor(ts.TotalHours):00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";

        private static string FormatTimeSrt(TimeSpan ts) =>
            $"{Math.Floor(ts.TotalHours):00}:{ts.Minutes:00}:{ts.Seconds:00},{ts.Milliseconds:000}";

        public void Dispose()
        {
            _factory?.Dispose();
            _semaphore.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            Dispose();
            await Task.CompletedTask;
        }
    }
}
