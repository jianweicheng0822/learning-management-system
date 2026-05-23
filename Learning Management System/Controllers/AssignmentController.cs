using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public class AssignmentController(IAssignmentService assignmentService, ICourseService courseService) : BaseController
{
    public async Task<IActionResult> Index(int courseId, int page = 1, int pageSize = 10)
    {
        var result = await assignmentService.GetByCourseAsync(courseId, page, pageSize);
        return HandleResult(result, assignments =>
        {
            ViewBag.CourseId = courseId;
            return View(assignments);
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await assignmentService.GetByIdAsync(id);
        return HandleResult(result, assignment => View(assignment));
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Create(int courseId)
    {
        var result = await courseService.GetByIdAsync(courseId);
        return HandleResult(result, course =>
        {
            ViewBag.CourseId = courseId;
            ViewBag.CourseName = course.Title;
            return View(new CreateAssignmentRequest());
        });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int courseId, CreateAssignmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CourseId = courseId;
            return View(request);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await assignmentService.CreateAsync(courseId, userId, User.IsInRole("Admin"), request);
        return HandleResult(result, assignment =>
        {
            TempData["Success"] = "Assignment created successfully.";
            return RedirectToAction("Details", new { id = assignment.Id });
        });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await assignmentService.GetByIdAsync(id);
        return HandleResult(result, assignment =>
        {
            var request = new UpdateAssignmentRequest
            {
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate
            };
            ViewBag.AssignmentId = id;
            ViewBag.CourseId = assignment.CourseId;
            return View(request);
        });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateAssignmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.AssignmentId = id;
            return View(request);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await assignmentService.UpdateAsync(id, userId, User.IsInRole("Admin"), request);
        return HandleResult(result, _ =>
        {
            TempData["Success"] = "Assignment updated successfully.";
            return RedirectToAction("Details", new { id });
        });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await assignmentService.GetByIdAsync(id);
        return HandleResult(result, assignment => View(assignment));
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var assignmentResult = await assignmentService.GetByIdAsync(id);
        if (!assignmentResult.IsSuccess)
            return HandleResult(assignmentResult, _ => View());

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await assignmentService.DeleteAsync(id, userId, User.IsInRole("Admin"));
        return HandleResult(result, () =>
        {
            TempData["Success"] = "Assignment deleted successfully.";
            return RedirectToAction("Index", new { courseId = assignmentResult.Value!.CourseId });
        });
    }
}
