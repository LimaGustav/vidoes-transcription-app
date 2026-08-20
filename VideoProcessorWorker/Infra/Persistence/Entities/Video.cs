using System;
using System.Collections.Generic;

namespace VideoProcessorWorker.Infra.Persistence.Entities;

public partial class Video
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public int? CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string MinioBucket { get; set; } = null!;

    public string MinioObjectKey { get; set; } = null!;

    public string? ThumbnailBucket { get; set; }

    public string? ThumbnailObjectKey { get; set; }

    public string? ContentType { get; set; }

    public long? SizeBytes { get; set; }

    public int? DurationSeconds { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? IdpotencyKey { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<VideoChunk> VideoChunks { get; set; } = new List<VideoChunk>();
}
