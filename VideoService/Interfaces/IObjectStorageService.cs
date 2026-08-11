namespace VideoService.Interfaces
{
    public interface IObjectStorageService
    {
        Task<string> GeneratePresignedUploadUrlAsync(string bucket, string objectKey, TimeSpan expiry, string contentType);
        Task<string> GeneratePresignedDownloadUrlAsync(string bucket, string objectKey, TimeSpan expiry);
        Task<bool> ObjectExistsAsync(string bucket, string objectKey, CancellationToken cancellationToken = default);
    }
}
