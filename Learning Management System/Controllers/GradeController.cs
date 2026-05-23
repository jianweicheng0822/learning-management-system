using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public class GradeController(
    IGradeService gradeService,
    ISubmissionService submissionService) : BaseController
{
    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Grade(int submissionId)
    {
        var result = await submissionService.GetByIdAsync(submissionId);
        return HandleResult(result, submission =>
        {
            ViewBag.SubmissionId = submissionId;
            ViewBag.StudentName = submission.StudentName;
            ViewBag.AssignmentTitle = submission.AssignmentTitle;
            return View(new GradeSubmissionRequest());
        });
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
        var result = await gradeService.GradeSubmissionAsync(submissionId, userId, User.IsInRole("Admin"), request);
        return HandleResultWithFeedback(result,
            _ =>
            {
                TempData["Success"] = "Submission graded successfully.";
                return RedirectToAction("Details", "Submission", new { id = submissionId });
            },
            () => RedirectToAction("Details", "Submission", new { id = submissionId }));
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int submissionId)
    {
        var result = await submissionService.GetByIdAsync(submissionId);
        return HandleResult(result, submission =>
        {
            ViewBag.SubmissionId = submissionId;
            ViewBag.StudentName = submission.StudentName;
            ViewBag.AssignmentTitle = submission.AssignmentTitle;

            var request = new GradeSubmissionRequest
            {
                Score = submission.Grade?.Score ?? 0,
                Feedback = submission.Grade?.Feedback
            };
            return View(request);
        });
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
        var result = await gradeService.UpdateGradeAsync(submissionId, userId, User.IsInRole("Admin"), request);
        return HandleResult(result, _ =>
        {
            TempData["Success"] = "Grade updated successfully.";
            return RedirectToAction("Details", "Submission", new { id = submissionId });
        });
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyGrades(int courseId, int page = 1, int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await gradeService.GetGradesForStudentCourseAsync(userId, courseId, page, pageSize);
        var average = await gradeService.GetAverageScoreAsync(userId, courseId);
        return HandleResult(result, grades =>
        {
            ViewBag.CourseId = courseId;
            ViewBag.AverageScore = average;
            return View(grades);
        });
    }
}
