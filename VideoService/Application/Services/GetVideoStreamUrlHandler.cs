using VideoService.Constants;
using VideoService.DTOs;
using VideoService.Interfaces;

namespace VideoService.Application.Services
{
    // Application/Services/GetVideoStreamUrlHandler.cs
    public class GetVideoStreamUrlHandler
    {
        private readonly IObjectStorageService _storage;
        private readonly IVideoRepository _videoRepository;
        private static readonly TimeSpan StreamUrlExpiry = TimeSpan.FromHours(2);

        public GetVideoStreamUrlHandler(IObjectStorageService storage, IVideoRepository videoRepository)
        {
            _storage = storage;
            _videoRepository = videoRepository;
        }

        public async Task<StreamVideoResponse> HandleAsync(Guid videoId, CancellationToken cancellationToken)
        {
            var video = await _videoRepository.GetByIdAsync(videoId, cancellationToken)
                ?? throw new FileNotFoundException($"Video {videoId} not found");

            if (video.Status != VideoStatusConstants.Ready)
                throw new InvalidOperationException($"Video {videoId} is not ready for streaming (status: {video.Status})");

            var streamUrl = await _storage.GeneratePresignedDownloadUrlAsync(
                video.MinioBucket, video.MinioObjectKey, StreamUrlExpiry);

            return new StreamVideoResponse
            {
                StreamUrl = streamUrl,
                ExpiresAt = DateTime.UtcNow.Add(StreamUrlExpiry)
            };
        }
    }
}
