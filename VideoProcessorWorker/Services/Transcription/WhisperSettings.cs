namespace VideoProcessorWorker.Services.Transcription
{
    public class WhisperSettings
    {
        /// <summary>
        /// Caminho para o arquivo de modelo ggml (ex: ggml-base.bin, ggml-medium.bin).
        /// </summary>
        public string ModelPath { get; set; } = "ggml-medium.bin";

        /// <summary>
        /// Tipo de modelo para download automático se o arquivo ModelPath não existir (ex: 'base', 'small', 'medium', 'large').
        /// </summary>
        public string AutoDownloadModelType { get; set; } = "medium";

        /// <summary>
        /// Idioma padrão para transcrição ('auto' para detecção de linguagem ou 'pt', 'en', etc.).
        /// </summary>
        public string DefaultLanguage { get; set; } = "auto";

        /// <summary>
        /// Tenta utilizar a GPU CUDA se disponível.
        /// </summary>
        public bool UseGpu { get; set; } = true;
    }
}
