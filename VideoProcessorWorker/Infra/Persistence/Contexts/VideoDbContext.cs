using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VideoProcessorWorker.Infra.Persistence.Entities;

namespace VideoProcessorWorker.Infra.Persistence.Contexts;

public partial class VideoDbContext : DbContext
{
    public VideoDbContext()
    {
    }

    public VideoDbContext(DbContextOptions<VideoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Video> Videos { get; set; }

    public virtual DbSet<VideoChunk> VideoChunks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5434;Database=video_db;Username=video_user;Password=video_pass");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.HasIndex(e => e.Name, "categories_name_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("videos_pkey");

            entity.ToTable("videos");

            entity.HasIndex(e => e.CategoryId, "idx_videos_category_id");

            entity.HasIndex(e => e.OwnerId, "idx_videos_owner_id");

            entity.HasIndex(e => e.Status, "idx_videos_status");

            entity.HasIndex(e => new { e.MinioBucket, e.MinioObjectKey }, "uq_videos_bucket_key").IsUnique();

            entity.HasIndex(e => e.IdpotencyKey, "videos_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ContentType)
                .HasMaxLength(100)
                .HasColumnName("content_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.IdpotencyKey)
                .HasColumnType("character varying")
                .HasColumnName("idpotency_key");
            entity.Property(e => e.MinioBucket)
                .HasMaxLength(100)
                .HasColumnName("minio_bucket");
            entity.Property(e => e.MinioObjectKey)
                .HasMaxLength(500)
                .HasColumnName("minio_object_key");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.SizeBytes).HasColumnName("size_bytes");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Uploaded'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.ThumbnailBucket)
                .HasMaxLength(100)
                .HasColumnName("thumbnail_bucket");
            entity.Property(e => e.ThumbnailObjectKey)
                .HasMaxLength(500)
                .HasColumnName("thumbnail_object_key");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Videos)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("videos_category_id_fkey");
        });

        modelBuilder.Entity<VideoChunk>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("video_chunks_pkey");

            entity.ToTable("video_chunks");

            entity.HasIndex(e => e.VideoId, "idx_video_chunks_video_id");

            entity.HasIndex(e => new { e.VideoId, e.ChunkIndex }, "uq_video_chunks_video_index").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.ChunkIndex).HasColumnName("chunk_index");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DurationSeconds)
                .HasPrecision(10, 3)
                .HasColumnName("duration_seconds");
            entity.Property(e => e.MinioBucket)
                .HasMaxLength(100)
                .HasColumnName("minio_bucket");
            entity.Property(e => e.MinioObjectKey)
                .HasMaxLength(500)
                .HasColumnName("minio_object_key");
            entity.Property(e => e.SizeBytes).HasColumnName("size_bytes");
            entity.Property(e => e.VideoId).HasColumnName("video_id");

            entity.HasOne(d => d.Video).WithMany(p => p.VideoChunks)
                .HasForeignKey(d => d.VideoId)
                .HasConstraintName("video_chunks_video_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
