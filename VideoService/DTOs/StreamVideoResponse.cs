namespace VideoService.DTOs
{
    public class StreamVideoResponse
    {
        public string StreamUrl { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
