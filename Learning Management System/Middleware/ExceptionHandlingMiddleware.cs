using System.Security.Claims;

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
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            logger.LogError(ex, "Unhandled exception on {Method} {Path} for user {UserId}",
                context.Request.Method, context.Request.Path, userId);
            context.Response.StatusCode = 500;
            context.Response.Redirect("/Home/Error?statusCode=500");
        }
    }
}
