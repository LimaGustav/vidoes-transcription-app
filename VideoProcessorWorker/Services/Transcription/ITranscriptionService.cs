namespace VideoProcessorWorker.Services.Transcription
{
    public interface ITranscriptionService
    {
        /// <summary>
        /// Transcreve um arquivo de áudio WAV (16kHz PCM mono) utilizando o modelo Whisper.
        /// </summary>
        /// <param name="audioWavPath">Caminho para o arquivo WAV.</param>
        /// <param name="language">Código do idioma (ex: 'pt', 'en') ou 'auto' para detecção automática.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>Resultado da transcrição com segmentos e formatos VTT/SRT gerados.</returns>
        Task<TranscriptionResult> TranscribeAsync(string audioWavPath, string language = "auto", CancellationToken cancellationToken = default);

        /// <summary>
        /// Transcreve a partir de um Stream de áudio.
        /// </summary>
        Task<TranscriptionResult> TranscribeAsync(Stream audioStream, string language = "auto", CancellationToken cancellationToken = default);
    }
}
