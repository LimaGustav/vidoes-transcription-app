using System.Text.Json.Serialization;

namespace VideoProcessorWorker.Application.GenerateSubtitles
{
    public class MinioNotificationEvent
    {
        [JsonPropertyName("EventName")]
        public string? EventName { get; set; }

        [JsonPropertyName("Key")]
        public string? Key { get; set; }

        [JsonPropertyName("Records")]
        public List<MinioRecord> Records { get; set; } = new();
    }

    public class MinioRecord
    {
        [JsonPropertyName("eventVersion")]
        public string? EventVersion { get; set; }

        [JsonPropertyName("eventSource")]
        public string? EventSource { get; set; }

        [JsonPropertyName("awsRegion")]
        public string? AwsRegion { get; set; }

        [JsonPropertyName("eventTime")]
        public string? EventTime { get; set; }

        [JsonPropertyName("eventName")]
        public string? EventName { get; set; }

        [JsonPropertyName("userIdentity")]
        public MinioUserIdentity? UserIdentity { get; set; }

        [JsonPropertyName("requestParameters")]
        public MinioRequestParameters? RequestParameters { get; set; }

        [JsonPropertyName("responseElements")]
        public Dictionary<string, string>? ResponseElements { get; set; }

        [JsonPropertyName("s3")]
        public MinioS3Entity? S3 { get; set; }

        public string? GetRequestId()
        {
            if (ResponseElements != null && ResponseElements.TryGetValue("x-amz-request-id", out var reqId))
            {
                return reqId;
            }

            return null;
        }
    }

    public class MinioUserIdentity
    {
        [JsonPropertyName("principalId")]
        public string? PrincipalId { get; set; }
    }

    public class MinioRequestParameters
    {
        [JsonPropertyName("principalId")]
        public string? PrincipalId { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("sourceIPAddress")]
        public string? SourceIPAddress { get; set; }
    }

    public class MinioS3Entity
    {
        [JsonPropertyName("s3SchemaVersion")]
        public string? S3SchemaVersion { get; set; }

        [JsonPropertyName("configurationId")]
        public string? ConfigurationId { get; set; }

        [JsonPropertyName("bucket")]
        public MinioBucketEntity? Bucket { get; set; }

        [JsonPropertyName("object")]
        public MinioObjectEntity? Object { get; set; }
    }

    public class MinioBucketEntity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("ownerIdentity")]
        public MinioUserIdentity? OwnerIdentity { get; set; }

        [JsonPropertyName("arn")]
        public string? Arn { get; set; }
    }

    public class MinioObjectEntity
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("eTag")]
        public string? ETag { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }

        [JsonPropertyName("userMetadata")]
        public Dictionary<string, string>? UserMetadata { get; set; }

        [JsonPropertyName("sequencer")]
        public string? Sequencer { get; set; }

        public string GetDecodedKey()
        {
            return string.IsNullOrWhiteSpace(Key) ? string.Empty : Uri.UnescapeDataString(Key);
        }
    }
}
