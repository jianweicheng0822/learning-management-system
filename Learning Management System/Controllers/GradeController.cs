using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public class GradeController(
    IGradeService gradeService,
    ISubmissionService submissionService) : Controller
{
    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Grade(int submissionId)
    {
        var submission = await submissionService.GetByIdAsync(submissionId);
        ViewBag.SubmissionId = submissionId;
        ViewBag.StudentName = submission.StudentName;
        ViewBag.AssignmentTitle = submission.AssignmentTitle;
        return View(new GradeSubmissionRequest());
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grade(int submissionId, GradeSubmissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.SubmissionId = submissionId;
            return View(request);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await gradeService.GradeSubmissionAsync(submissionId, userId, request);
        TempData["Success"] = "Submission graded successfully.";
        return RedirectToAction("Details", "Submission", new { id = submissionId });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int submissionId)
    {
        var submission = await submissionService.GetByIdAsync(submissionId);
        ViewBag.SubmissionId = submissionId;
        ViewBag.StudentName = submission.StudentName;
        ViewBag.AssignmentTitle = submission.AssignmentTitle;

        var request = new GradeSubmissionRequest
        {
            Score = submission.Grade?.Score ?? 0,
            Feedback = submission.Grade?.Feedback
        };
        return View(request);
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int submissionId, GradeSubmissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.SubmissionId = submissionId;
            return View(request);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await gradeService.UpdateGradeAsync(submissionId, userId, request);
        TempData["Success"] = "Grade updated successfully.";
        return RedirectToAction("Details", "Submission", new { id = submissionId });
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyGrades(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var grades = await gradeService.GetGradesForStudentCourseAsync(userId, courseId);
        ViewBag.CourseId = courseId;
        return View(grades);
    }
}
