namespace LMS.Services;

public static class FileValidationHelper
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".txt", ".pptx", ".xlsx",
        ".py", ".java", ".cs", ".js", ".ts",
        ".zip", ".tar.gz"
    };

    public const long MaxFileSize = 25 * 1024 * 1024; // 25 MB

    public static (bool IsValid, string? ErrorMessage) Validate(string fileName, long fileSize)
    {
        var extension = GetExtension(fileName);

        if (!AllowedExtensions.Contains(extension))
            return (false, $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions.Order())}");

        if (fileSize > MaxFileSize)
            return (false, $"File size exceeds the maximum allowed size of 25 MB.");

        if (fileSize == 0)
            return (false, "File is empty.");

        return (true, null);
    }

    public static string GenerateS3Key(int courseId, int assignmentId, string studentId, string fileName)
    {
        var extension = GetExtension(fileName);
        return $"courses/{courseId}/assignments/{assignmentId}/students/{studentId}/{Guid.NewGuid()}{extension}";
    }

    // Path.GetExtension only returns ".gz" for .tar.gz files, so handle that edge case explicitly
    private static string GetExtension(string fileName)
    {
        if (fileName.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            return ".tar.gz";

        return Path.GetExtension(fileName);
    }
}
