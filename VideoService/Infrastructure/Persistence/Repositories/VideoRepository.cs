using Microsoft.EntityFrameworkCore;
using VideoService.Domains;
using VideoService.Infrastructure.Persistence.Contexts;
using VideoService.Interfaces;

namespace VideoService.Infrastructure.Persistence.Repositories
{
    // Infrastructure/Repositories/VideoRepository.cs
    public class VideoRepository : IVideoRepository
    {
        private readonly VideoDbContext _context;

        public VideoRepository(VideoDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Video video, CancellationToken cancellationToken)
        {
            _context.Videos.Add(video);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Video?> GetByIdAsync(Guid videoId, CancellationToken cancellationToken)
        {
            return await _context.Videos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == videoId, cancellationToken);
        }
    }
}
