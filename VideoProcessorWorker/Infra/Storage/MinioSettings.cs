namespace VideoProcessorWorker.Infra.Storage
{
    public class MinioSettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = false;
        public string VideosBucket { get; set; } = "videos";
        public string ProcessedBucket { get; set; } = "processed-videos";
    }
}
