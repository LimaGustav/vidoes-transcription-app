namespace VideoProcessorWorker.Services.Audio
{
    public interface IAudioExtractionService
    {
        /// <summary>
        /// Extrai o áudio de um vídeo e converte para WAV PCM 16kHz mono (formato ideal para transcrição com Whisper).
        /// Tenta aceleração por GPU (CUDA/CUVID) se disponível/habilitado, com fallback automático para CPU.
        /// </summary>
        Task<string> ExtractAudioForWhisperAsync(string videoPath, string? outputWavPath = null, CancellationToken cancellationToken = default);
    }
}
