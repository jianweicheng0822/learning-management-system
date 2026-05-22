using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

// Assignment browsing (all users) and CRUD (Instructor/Admin only)
public class AssignmentController(IAssignmentService assignmentService, ICourseService courseService) : Controller
{
    // List all assignments for a course
    public async Task<IActionResult> Index(int courseId)
    {
        var assignments = await assignmentService.GetByCourseAsync(courseId);
        ViewBag.CourseId = courseId;
        return View(assignments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var assignment = await assignmentService.GetByIdAsync(id);
        return View(assignment);
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Create(int courseId)
    {
        var course = await courseService.GetByIdAsync(courseId);
        ViewBag.CourseId = courseId;
        ViewBag.CourseName = course.Title;
        return View(new CreateAssignmentRequest());
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
        var assignment = await assignmentService.CreateAsync(courseId, userId, User.IsInRole("Admin"), request);
        TempData["Success"] = "Assignment created successfully.";
        return RedirectToAction("Details", new { id = assignment.Id });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var assignment = await assignmentService.GetByIdAsync(id);
        var request = new UpdateAssignmentRequest
        {
            Title = assignment.Title,
            Description = assignment.Description,
            DueDate = assignment.DueDate
        };
        ViewBag.AssignmentId = id;
        ViewBag.CourseId = assignment.CourseId;
        return View(request);
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
        await assignmentService.UpdateAsync(id, userId, User.IsInRole("Admin"), request);
        TempData["Success"] = "Assignment updated successfully.";
        return RedirectToAction("Details", new { id });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment = await assignmentService.GetByIdAsync(id);
        return View(assignment);
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var assignment = await assignmentService.GetByIdAsync(id);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await assignmentService.DeleteAsync(id, userId, User.IsInRole("Admin"));
        TempData["Success"] = "Assignment deleted successfully.";
        return RedirectToAction("Index", new { courseId = assignment.CourseId });
    }
}
