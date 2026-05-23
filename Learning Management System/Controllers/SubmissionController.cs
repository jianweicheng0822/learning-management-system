using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public class SubmissionController(
    ISubmissionService submissionService,
    IAssignmentService assignmentService) : BaseController
{
    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> Create(int assignmentId)
    {
        var result = await assignmentService.GetByIdAsync(assignmentId);
        return HandleResult(result, assignment =>
        {
            ViewBag.AssignmentId = assignmentId;
            ViewBag.AssignmentTitle = assignment.Title;
            ViewBag.CourseName = assignment.CourseName;
            return View(new CreateSubmissionRequest());
        });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int assignmentId, CreateSubmissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AssignmentId = assignmentId;
            return View(request);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await submissionService.SubmitAsync(assignmentId, userId, request);
        return HandleResultWithFeedback(result,
            submission =>
            {
                TempData["Success"] = "Assignment submitted successfully.";
                return RedirectToAction("Details", new { id = submission.Id });
            },
            () => RedirectToAction("Details", "Assignment", new { id = assignmentId }));
    }

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Index(int assignmentId, int page = 1, int pageSize = 10)
    {
        var assignmentResult = await assignmentService.GetByIdAsync(assignmentId);
        if (!assignmentResult.IsSuccess)
            return HandleResult(assignmentResult, _ => View());

        var result = await submissionService.GetByAssignmentAsync(assignmentId, page, pageSize);
        return HandleResult(result, submissions =>
        {
            ViewBag.AssignmentId = assignmentId;
            ViewBag.AssignmentTitle = assignmentResult.Value!.Title;
            ViewBag.CourseId = assignmentResult.Value.CourseId;
            return View(submissions);
        });
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var result = await submissionService.GetByIdAsync(id);
        return HandleResult(result, submission => View(submission));
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MySubmissions(int page = 1, int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await submissionService.GetByStudentAsync(userId, page, pageSize);
        return HandleResult(result, submissions => View(submissions));
    }
}
