using VideoService.Infrastructure.Persistence.Entities;

namespace VideoService.Interfaces
{
    public interface IVideoRepository
    {
        Task CreateAsync(Video video, CancellationToken cancellationToken);
        Task<Video?> GetByIdAsync(Guid videoId, CancellationToken cancellationToken);
    }
}
