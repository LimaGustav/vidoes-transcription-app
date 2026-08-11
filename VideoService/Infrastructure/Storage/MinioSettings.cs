namespace VideoService.Infrastructure.Storage
{
    // Infrastructure/MinioSettings.cs
    public class MinioSettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = false;
    }
}
