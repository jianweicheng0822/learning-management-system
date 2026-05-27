using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public class SubmissionController(
    ISubmissionService submissionService,
    IAssignmentService assignmentService,
    IFileStorageService fileStorage) : BaseController
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var existingSubmission = submissionService.GetByStudentForAssignmentAsync(assignmentId, userId).Result;
            if (existingSubmission is not null)
            {
                ViewBag.ExistingSubmission = new
                {
                    SubmittedAt = existingSubmission.SubmittedAt,
                    OriginalFileName = existingSubmission.OriginalFileName,
                    IsGraded = existingSubmission.Grade is not null
                };
            }

            return View(new CreateSubmissionRequest());
        });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int assignmentId, CreateSubmissionRequest request, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AssignmentId = assignmentId;
            return View(request);
        }

        string? s3Key = null;
        string? originalFileName = null;

        if (file is { Length: > 0 })
        {
            var (isValid, errorMessage) = FileValidationHelper.Validate(file.FileName, file.Length);
            if (!isValid)
            {
                TempData["Error"] = errorMessage;
                ViewBag.AssignmentId = assignmentId;
                return View(request);
            }

            var assignmentResult = await assignmentService.GetByIdAsync(assignmentId);
            if (!assignmentResult.IsSuccess)
                return HandleResult(assignmentResult, _ => View());

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            s3Key = FileValidationHelper.GenerateS3Key(assignmentResult.Value!.CourseId, assignmentId, userId, file.FileName);
            originalFileName = file.FileName;

            await using var stream = file.OpenReadStream();
            await fileStorage.UploadAsync(stream, s3Key, file.ContentType);
        }

        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await submissionService.SubmitAsync(assignmentId, userId, request, s3Key, originalFileName);
            return HandleResultWithFeedback(result,
                submission =>
                {
                    TempData["Success"] = "Assignment submitted successfully.";
                    return RedirectToAction("Details", new { id = submission.Id });
                },
                () => RedirectToAction("Details", "Assignment", new { id = assignmentId }));
        }
        catch
        {
            // Clean up uploaded file if submission fails
            if (s3Key != null)
                await fileStorage.DeleteAsync(s3Key);
            throw;
        }
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

    [Authorize]
    public async Task<IActionResult> Download(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isInstructorOrAdmin = User.IsInRole("Instructor") || User.IsInRole("Admin");
        var result = await submissionService.GetDownloadUrlAsync(id, userId, isInstructorOrAdmin);

        return HandleResult(result, url => Redirect(url));
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MySubmissions(int page = 1, int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await submissionService.GetByStudentAsync(userId, page, pageSize);
        return HandleResult(result, submissions => View(submissions));
    }
}
