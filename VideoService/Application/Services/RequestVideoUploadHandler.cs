using VideoService.Domains;
using VideoService.DTOs;
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
                Status = VideoStatus.PendingUpload,
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

        private static string GetExtension(string contentType) => contentType switch
        {
            "video/mp4" => ".mp4",
            "video/webm" => ".webm",
            "video/quicktime" => ".mov",
            _ => throw new ArgumentException($"Unsupported content type: {contentType}")
        };
    }
}
