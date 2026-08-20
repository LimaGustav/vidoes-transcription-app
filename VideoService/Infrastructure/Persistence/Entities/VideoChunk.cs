using System;
using System.Collections.Generic;

namespace VideoService.Infrastructure.Persistence.Entities;

public partial class VideoChunk
{
    public Guid Id { get; set; }

    public Guid VideoId { get; set; }

    public int ChunkIndex { get; set; }

    public string MinioBucket { get; set; } = null!;

    public string MinioObjectKey { get; set; } = null!;

    public long? SizeBytes { get; set; }

    public decimal? DurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Video Video { get; set; } = null!;
}
