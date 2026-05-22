namespace LMS.ViewModels;

// Passed to the error view to display status code, message, and request trace ID
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public int StatusCode { get; set; }
    public string Message { get; set; } = "An unexpected error occurred.";
}
