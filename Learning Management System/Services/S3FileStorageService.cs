using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace LMS.Services;

public class S3Settings
{
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
}

public class S3FileStorageService(IAmazonS3 s3Client, IOptions<S3Settings> settings) : IFileStorageService
{
    private readonly S3Settings _settings = settings.Value;

    public async Task<string> UploadAsync(Stream fileStream, string key, string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType
        };

        await s3Client.PutObjectAsync(request);
        return key;
    }

    public string GetDownloadUrl(string key, string originalFileName, TimeSpan? expiry = null)
    {
        var duration = expiry ?? TimeSpan.FromMinutes(15);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(duration),
            ResponseHeaderOverrides =
            {
                ContentDisposition = $"attachment; filename=\"{originalFileName}\""
            }
        };

        return s3Client.GetPreSignedURL(request);
    }

    public async Task DeleteAsync(string key)
    {
        await s3Client.DeleteObjectAsync(_settings.BucketName, key);
    }
}
