using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VideoProcessorWorker.Infra.Storage
{
    public class MinioObjectStorageService : IObjectStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<MinioObjectStorageService> _logger;

        public MinioObjectStorageService(IOptions<MinioSettings> options, ILogger<MinioObjectStorageService> logger)
        {
            var settings = options.Value;
            _logger = logger;

            var config = new AmazonS3Config
            {
                ServiceURL = $"{(settings.UseSsl ? "https" : "http")}://{settings.Endpoint}",
                ForcePathStyle = true,
                UseHttp = !settings.UseSsl
            };

            _s3Client = new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
        }

        public async Task<Stream> GetObjectStreamAsync(string bucket, string objectKey, CancellationToken cancellationToken = default)
        {
            var response = await _s3Client.GetObjectAsync(bucket, objectKey, cancellationToken);
            return response.ResponseStream;
        }

        public async Task DownloadObjectToFileAsync(string bucket, string objectKey, string localFilePath, CancellationToken cancellationToken = default)
        {
            var directory = Path.GetDirectoryName(localFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var request = new GetObjectRequest
            {
                BucketName = bucket,
                Key = objectKey
            };

            using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            await response.WriteResponseStreamToFileAsync(localFilePath, append: false, cancellationToken);
            _logger.LogInformation("Downloaded {Bucket}/{Key} to {LocalPath}", bucket, objectKey, localFilePath);
        }

        public async Task UploadFileAsync(string bucket, string objectKey, string localFilePath, string contentType, CancellationToken cancellationToken = default)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = bucket,
                Key = objectKey,
                FilePath = localFilePath,
                ContentType = contentType
            };

            await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);
            _logger.LogInformation("Uploaded {LocalPath} to {Bucket}/{Key}", localFilePath, bucket, objectKey);
        }

        public async Task UploadStreamAsync(string bucket, string objectKey, Stream stream, string contentType, CancellationToken cancellationToken = default)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = bucket,
                Key = objectKey,
                InputStream = stream,
                ContentType = contentType
            };

            await fileTransferUtility.UploadAsync(uploadRequest, cancellationToken);
            _logger.LogInformation("Uploaded stream to {Bucket}/{Key}", bucket, objectKey);
        }

        public Task<string> GeneratePresignedDownloadUrlAsync(string bucket, string objectKey, TimeSpan expiry)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiry)
            };

            return Task.FromResult(_s3Client.GetPreSignedURL(request));
        }

        public Task<string> GeneratePresignedUploadUrlAsync(string bucket, string objectKey, TimeSpan expiry, string contentType)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.Add(expiry),
                ContentType = contentType,
                Protocol = Protocol.HTTP
            };

            return Task.FromResult(_s3Client.GetPreSignedURL(request));
        }

        public async Task<bool> ObjectExistsAsync(string bucket, string objectKey, CancellationToken cancellationToken = default)
        {
            try
            {
                await _s3Client.GetObjectMetadataAsync(bucket, objectKey, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task DeleteObjectAsync(string bucket, string objectKey, CancellationToken cancellationToken = default)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = objectKey
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            _logger.LogInformation("Deleted object {Bucket}/{Key}", bucket, objectKey);
        }
    }
}
