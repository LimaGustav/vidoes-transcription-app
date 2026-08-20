using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace VideoProcessorWorker.Services.Audio
{
    public class FfmpegAudioExtractionService : IAudioExtractionService
    {
        private readonly FfmpegSettings _settings;
        private readonly ILogger<FfmpegAudioExtractionService> _logger;

        public FfmpegAudioExtractionService(IOptions<FfmpegSettings> options, ILogger<FfmpegAudioExtractionService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<string> ExtractAudioForWhisperAsync(string videoPath, string? outputWavPath = null, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException($"Arquivo de vídeo não encontrado: {videoPath}");
            }

            if (string.IsNullOrWhiteSpace(outputWavPath))
            {
                var directory = Path.GetDirectoryName(videoPath) ?? Path.GetTempPath();
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(videoPath);
                outputWavPath = Path.Combine(directory, $"{fileNameWithoutExt}_{Guid.NewGuid():N}.wav");
            }

            // Garante que a pasta destino exista
            var outDir = Path.GetDirectoryName(outputWavPath);
            if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            // Tenta decodificação com aceleração por GPU (CUDA) se configurado
            if (_settings.EnableGpuAcceleration)
            {
                var gpuArgs = $"-y -hwaccel cuda -i \"{videoPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputWavPath}\"";
                _logger.LogInformation("Tentando extrair áudio com aceleração CUDA via FFmpeg: {Args}", gpuArgs);
                
                var success = await RunFfmpegAsync(gpuArgs, cancellationToken);
                if (success && File.Exists(outputWavPath) && new FileInfo(outputWavPath).Length > 0)
                {
                    _logger.LogInformation("Áudio extraído com sucesso via GPU: {OutputPath}", outputWavPath);
                    return outputWavPath;
                }

                _logger.LogWarning("Falha ao usar aceleração por GPU no FFmpeg. Realizando fallback para CPU...");
            }

            // Fallback para CPU
            var cpuArgs = $"-y -i \"{videoPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputWavPath}\"";
            _logger.LogInformation("Executando extração de áudio via CPU no FFmpeg: {Args}", cpuArgs);

            var cpuSuccess = await RunFfmpegAsync(cpuArgs, cancellationToken);
            if (!cpuSuccess || !File.Exists(outputWavPath) || new FileInfo(outputWavPath).Length == 0)
            {
                throw new InvalidOperationException($"Falha ao extrair áudio do vídeo {videoPath} usando FFmpeg.");
            }

            _logger.LogInformation("Áudio extraído com sucesso via CPU: {OutputPath}", outputWavPath);
            return outputWavPath;
        }

        private async Task<bool> RunFfmpegAsync(string arguments, CancellationToken cancellationToken)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = string.IsNullOrWhiteSpace(_settings.FfmpegPath) ? "ffmpeg" : _settings.FfmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = processInfo };
                process.Start();

                var errorOutputTask = process.StandardError.ReadToEndAsync(cancellationToken);
                var stdOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken);

                var errorOutput = await errorOutputTask;
                if (process.ExitCode != 0)
                {
                    _logger.LogWarning("FFmpeg finalizou com código {ExitCode}. Erro: {Error}", process.ExitCode, errorOutput);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao tentar executar processo FFmpeg.");
                return false;
            }
        }
    }
}
