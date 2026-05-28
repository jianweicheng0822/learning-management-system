using LMS.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public abstract class BaseController : Controller
{
    protected IActionResult HandleResult<T>(ServiceResult<T> result, Func<T, IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value!);

        return ErrorResult(result);
    }

    protected IActionResult HandleResult(ServiceResult result, Func<IActionResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess();

        return ErrorResult(result);
    }

    // Conflict/ValidationError are user-facing recoverable errors — redirect with TempData message
    // so the user stays on the form. Other errors (NotFound, Unauthorized) get a dedicated error page.
    protected IActionResult HandleResultWithFeedback<T>(
        ServiceResult<T> result,
        Func<T, IActionResult> onSuccess,
        Func<IActionResult> onConflictRedirect)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value!);

        if (result.Error is ErrorType.Conflict or ErrorType.ValidationError)
        {
            TempData["Error"] = result.ErrorMessage;
            return onConflictRedirect();
        }

        return ErrorResult(result);
    }

    protected IActionResult HandleResultWithFeedback(
        ServiceResult result,
        Func<IActionResult> onSuccess,
        Func<IActionResult> onConflictRedirect)
    {
        if (result.IsSuccess)
            return onSuccess();

        if (result.Error is ErrorType.Conflict or ErrorType.ValidationError)
        {
            TempData["Error"] = result.ErrorMessage;
            return onConflictRedirect();
        }

        return ErrorResult(result);
    }

    // Maps service-layer error types to HTTP status codes for the generic error page
    private IActionResult ErrorResult(ServiceResult result)
    {
        var statusCode = result.Error switch
        {
            ErrorType.NotFound => 404,
            ErrorType.Unauthorized => 403,
            ErrorType.Conflict => 409,
            ErrorType.ValidationError => 400,
            _ => 500
        };

        return RedirectToAction("Error", "Home", new { statusCode });
    }
}
