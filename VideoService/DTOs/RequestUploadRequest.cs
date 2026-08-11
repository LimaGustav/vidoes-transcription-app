using System.ComponentModel.DataAnnotations;

namespace VideoService.DTOs
{
    public class RequestUploadRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string ContentType { get; set; } = string.Empty; // ex: "video/mp4"

        public int? CategoryId { get; set; }
    }
}
