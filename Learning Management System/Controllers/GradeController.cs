using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

// Grading actions for Instructor/Admin and grade viewing for Students
public class GradeController(
    IGradeService gradeService,
    ISubmissionService submissionService) : Controller
{
    // Show the grading form for a submission
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

    // Submit a new grade for a submission
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
        try
        {
            await gradeService.GradeSubmissionAsync(submissionId, userId, User.IsInRole("Admin"), request);
            TempData["Success"] = "Submission graded successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Details", "Submission", new { id = submissionId });
    }

    // Show the edit form pre-filled with the existing grade
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
        await gradeService.UpdateGradeAsync(submissionId, userId, User.IsInRole("Admin"), request);
        TempData["Success"] = "Grade updated successfully.";
        return RedirectToAction("Details", "Submission", new { id = submissionId });
    }

    // Student-only: view grades for all submissions in a specific course
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyGrades(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var grades = await gradeService.GetGradesForStudentCourseAsync(userId, courseId);
        ViewBag.CourseId = courseId;
        return View(grades);
    }
}
