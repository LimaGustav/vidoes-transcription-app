namespace VideoProcessorWorker.Infra.Messaging
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string? ConsumerGroupId { get; set; } = "video-processor-worker-group";
        public string MinioVideoUploadedTopic { get; set; } = "minio.video.uploaded";
    }
}

