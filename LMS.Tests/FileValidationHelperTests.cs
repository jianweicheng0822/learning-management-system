using LMS.Services;

namespace LMS.Tests;

public class FileValidationHelperTests
{
    [Theory]
    [InlineData("report.pdf")]
    [InlineData("essay.docx")]
    [InlineData("notes.txt")]
    [InlineData("slides.pptx")]
    [InlineData("data.xlsx")]
    [InlineData("script.py")]
    [InlineData("Main.java")]
    [InlineData("Program.cs")]
    [InlineData("app.js")]
    [InlineData("index.ts")]
    [InlineData("archive.zip")]
    [InlineData("archive.tar.gz")]
    public void Validate_AcceptsAllowedExtensions(string fileName)
    {
        var (isValid, errorMessage) = FileValidationHelper.Validate(fileName, 1024);

        Assert.True(isValid);
        Assert.Null(errorMessage);
    }

    [Theory]
    [InlineData("malware.exe")]
    [InlineData("script.sh")]
    [InlineData("image.png")]
    [InlineData("video.mp4")]
    [InlineData("noextension")]
    public void Validate_RejectsDisallowedExtensions(string fileName)
    {
        var (isValid, errorMessage) = FileValidationHelper.Validate(fileName, 1024);

        Assert.False(isValid);
        Assert.NotNull(errorMessage);
        Assert.Contains("not allowed", errorMessage);
    }

    [Fact]
    public void Validate_RejectsFilesExceedingMaxSize()
    {
        var (isValid, errorMessage) = FileValidationHelper.Validate("report.pdf", 26 * 1024 * 1024);

        Assert.False(isValid);
        Assert.Contains("25 MB", errorMessage!);
    }

    [Fact]
    public void Validate_RejectsEmptyFiles()
    {
        var (isValid, errorMessage) = FileValidationHelper.Validate("report.pdf", 0);

        Assert.False(isValid);
        Assert.Contains("empty", errorMessage!);
    }

    [Fact]
    public void Validate_AcceptsFileAtExactMaxSize()
    {
        var (isValid, _) = FileValidationHelper.Validate("report.pdf", FileValidationHelper.MaxFileSize);

        Assert.True(isValid);
    }

    [Fact]
    public void GenerateS3Key_ContainsExpectedSegments()
    {
        var key = FileValidationHelper.GenerateS3Key(10, 20, "stu-1", "homework.pdf");

        Assert.StartsWith("courses/10/assignments/20/students/stu-1/", key);
        Assert.EndsWith(".pdf", key);
    }

    [Fact]
    public void GenerateS3Key_HandlesTarGzExtension()
    {
        var key = FileValidationHelper.GenerateS3Key(1, 2, "stu-1", "archive.tar.gz");

        Assert.EndsWith(".tar.gz", key);
    }

    [Fact]
    public void GenerateS3Key_GeneratesUniqueKeys()
    {
        var key1 = FileValidationHelper.GenerateS3Key(1, 1, "stu-1", "file.pdf");
        var key2 = FileValidationHelper.GenerateS3Key(1, 1, "stu-1", "file.pdf");

        Assert.NotEqual(key1, key2);
    }
}
