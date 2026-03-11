using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public class SubmissionController(
    ISubmissionService submissionService,
    IAssignmentService assignmentService) : Controller
{
    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> Create(int assignmentId)
    {
        var assignment = await assignmentService.GetByIdAsync(assignmentId);
        ViewBag.AssignmentId = assignmentId;
        ViewBag.AssignmentTitle = assignment.Title;
        ViewBag.CourseName = assignment.CourseName;
        return View(new CreateSubmissionRequest());
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
        var submission = await submissionService.SubmitAsync(assignmentId, userId, request);
        TempData["Success"] = "Assignment submitted successfully.";
        return RedirectToAction("Details", new { id = submission.Id });
    }

    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Index(int assignmentId)
    {
        var submissions = await submissionService.GetByAssignmentAsync(assignmentId);
        var assignment = await assignmentService.GetByIdAsync(assignmentId);
        ViewBag.AssignmentId = assignmentId;
        ViewBag.AssignmentTitle = assignment.Title;
        ViewBag.CourseId = assignment.CourseId;
        return View(submissions);
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var submission = await submissionService.GetByIdAsync(id);
        return View(submission);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MySubmissions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var submissions = await submissionService.GetByStudentAsync(userId);
        return View(submissions);
    }
}
