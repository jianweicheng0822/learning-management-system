using System.Net;

namespace LMS.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }
    }

    private void HandleException(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
            InvalidOperationException => (int)HttpStatusCode.Conflict,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning(exception, "Handled exception ({StatusCode})", statusCode);

        context.Response.StatusCode = statusCode;

        // Store error details for the error view
        context.Items["ErrorMessage"] = exception.Message;
        context.Items["ErrorStatusCode"] = statusCode;

        // Re-throw to let the developer exception page or UseStatusCodePages handle it,
        // or redirect to the error action
        var path = $"/Home/Error?statusCode={statusCode}";
        context.Response.Redirect(path);
    }
}
