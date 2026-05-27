namespace LMS.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string key, string contentType);
    string GetDownloadUrl(string key, string originalFileName, TimeSpan? expiry = null);
    Task DeleteAsync(string key);
}
