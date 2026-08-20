namespace VideoProcessorWorker.Services.Transcription
{
    public class SubtitleSegment
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class TranscriptionResult
    {
        public string Language { get; set; } = string.Empty;
        public string FullText { get; set; } = string.Empty;
        public List<SubtitleSegment> Segments { get; set; } = new();
        public string VttContent { get; set; } = string.Empty;
        public string SrtContent { get; set; } = string.Empty;
    }
}
