namespace VideoService.DTOs
{
    public class RequestUploadResponse
    {
        public Guid VideoId { get; set; }
        public string UploadUrl { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
