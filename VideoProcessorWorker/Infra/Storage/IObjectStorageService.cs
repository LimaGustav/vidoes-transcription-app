namespace VideoProcessorWorker.Infra.Storage
{
    public interface IObjectStorageService
    {
        Task<Stream> GetObjectStreamAsync(string bucket, string objectKey, CancellationToken cancellationToken = default);
        Task DownloadObjectToFileAsync(string bucket, string objectKey, string localFilePath, CancellationToken cancellationToken = default);
        Task UploadFileAsync(string bucket, string objectKey, string localFilePath, string contentType, CancellationToken cancellationToken = default);
        Task UploadStreamAsync(string bucket, string objectKey, Stream stream, string contentType, CancellationToken cancellationToken = default);
        Task<string> GeneratePresignedDownloadUrlAsync(string bucket, string objectKey, TimeSpan expiry);
        Task<string> GeneratePresignedUploadUrlAsync(string bucket, string objectKey, TimeSpan expiry, string contentType);
        Task<bool> ObjectExistsAsync(string bucket, string objectKey, CancellationToken cancellationToken = default);
        Task DeleteObjectAsync(string bucket, string objectKey, CancellationToken cancellationToken = default);
    }
}
