using VideoService.Constants;
using VideoService.DTOs;
using VideoService.Infrastructure.Persistence.Entities;
using VideoService.Interfaces;

namespace VideoService.Application.Services
{
    // Application/Services/RequestVideoUploadHandler.cs
    public class RequestVideoUploadHandler
    {
        private readonly IObjectStorageService _storage;
        private readonly IVideoRepository _videoRepository;
        private const string Bucket = "videos";
        private static readonly TimeSpan UploadUrlExpiry = TimeSpan.FromMinutes(15);

        public RequestVideoUploadHandler(IObjectStorageService storage, IVideoRepository videoRepository)
        {
            _storage = storage;
            _videoRepository = videoRepository;
        }

        public async Task<RequestUploadResponse> HandleAsync(Guid ownerId, RequestUploadRequest request, CancellationToken cancellationToken)
        {
            var videoId = Guid.NewGuid();
            var objectKey = $"videos/{videoId}/original{GetExtension(request.ContentType)}";
            var now = DateTime.UtcNow;

            var video = new Video
            {
                Id = videoId,
                OwnerId = ownerId,
                CategoryId = request.CategoryId,
                Title = request.Title,
                Description = request.Description,
                MinioBucket = Bucket,
                MinioObjectKey = objectKey,
                ContentType = request.ContentType,
                Status = VideoStatusConstants.PendingUpload,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _videoRepository.CreateAsync(video, cancellationToken);

            var uploadUrl = await _storage.GeneratePresignedUploadUrlAsync(
                Bucket, objectKey, UploadUrlExpiry, request.ContentType);

            return new RequestUploadResponse
            {
                VideoId = videoId,
                UploadUrl = uploadUrl,
                ExpiresAt = DateTime.UtcNow.Add(UploadUrlExpiry)
            };
        }

        private static string GetExtension(string contentType) => contentType?.ToLowerInvariant() switch
        {
            "video/mp4" => ".mp4",
            "video/webm" => ".webm",
            "video/quicktime" => ".mov",
            "video/x-msvideo" or "video/avi" => ".avi",
            "video/x-matroska" or "video/mkv" => ".mkv",
            "video/x-ms-wmv" => ".wmv",
            "video/x-flv" or "video/flv" => ".flv",
            "video/mpeg" => ".mpeg",
            "video/3gpp" => ".3gp",
            "video/3gpp2" => ".3g2",
            "video/ogg" => ".ogv",
            "video/mp2t" => ".ts",
            "video/x-m4v" => ".m4v",
            _ => throw new ArgumentException($"Unsupported content type: {contentType}")
        };
    }
}
