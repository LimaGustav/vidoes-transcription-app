using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using VideoService.Interfaces;

namespace VideoService.Infrastructure.Storage
{
    // Infrastructure/MinioObjectStorageService.cs
public class MinioObjectStorageService : IObjectStorageService
{
    private readonly IAmazonS3 _s3Client;

    public MinioObjectStorageService(IOptions<MinioSettings> options)
    {
        var settings = options.Value;

        var config = new AmazonS3Config
        {
            ServiceURL = $"{(settings.UseSsl ? "https" : "http")}://{settings.Endpoint}",
            ForcePathStyle = true // required for MinIO (path-style, not virtual-hosted)
        };

        _s3Client = new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
    }

    public Task<string> GeneratePresignedUploadUrlAsync(string bucket, string objectKey, TimeSpan expiry, string contentType)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiry),
            ContentType = contentType
        };

        return Task.FromResult(_s3Client.GetPreSignedURL(request));
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
}
}
