using System.Diagnostics;
using LMS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

// Serves the landing page, privacy page, and generic error page
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    // Maps HTTP status codes to user-friendly error messages
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? 500,
            Message = statusCode switch
            {
                400 => "The request was invalid.",
                403 => "You do not have permission to perform this action.",
                404 => "The requested resource was not found.",
                409 => "The request conflicts with the current state.",
                _ => "An unexpected error occurred."
            }
        };

        return View(model);
    }
}
