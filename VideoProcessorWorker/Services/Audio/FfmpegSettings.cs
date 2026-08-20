namespace VideoProcessorWorker.Services.Audio
{
    public class FfmpegSettings
    {
        public string FfmpegPath { get; set; } = "ffmpeg";
        public bool EnableGpuAcceleration { get; set; } = true;
    }
}
