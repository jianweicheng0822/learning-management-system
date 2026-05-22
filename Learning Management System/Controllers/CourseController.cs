using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

// Course browsing, CRUD (Instructor/Admin), and enrollment actions (Student)
public class CourseController(ICourseService courseService) : Controller
{
    // List all courses — accessible to everyone
    public async Task<IActionResult> Index()
    {
        var courses = await courseService.GetAllAsync();
        return View(courses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var course = await courseService.GetByIdAsync(id);
        return View(course);
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateCourseRequest());
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var course = await courseService.CreateAsync(userId, request);
        TempData["Success"] = "Course created successfully.";
        return RedirectToAction("Details", new { id = course.Id });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await courseService.GetByIdAsync(id);
        var request = new UpdateCourseRequest
        {
            Title = course.Title,
            Description = course.Description
        };
        ViewBag.CourseId = id;
        return View(request);
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCourseRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CourseId = id;
            return View(request);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await courseService.UpdateAsync(id, userId, User.IsInRole("Admin"), request);
        TempData["Success"] = "Course updated successfully.";
        return RedirectToAction("Details", new { id });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await courseService.GetByIdAsync(id);
        return View(course);
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await courseService.DeleteAsync(id, userId, User.IsInRole("Admin"));
        TempData["Success"] = "Course deleted successfully.";
        return RedirectToAction("Index");
    }

    // Instructor-only: list courses taught by the current user
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> MyCourses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var courses = await courseService.GetByInstructorAsync(userId);
        return View(courses);
    }

    // Student-only: list courses the current student is enrolled in
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Enrolled()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var courses = await courseService.GetEnrolledCoursesAsync(userId);
        return View(courses);
    }

    // Student-only: enroll in a course
    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await courseService.EnrollStudentAsync(id, userId);
            TempData["Success"] = "Enrolled successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("Details", new { id });
    }

    // Student-only: drop a course
    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await courseService.UnenrollStudentAsync(id, userId);
        TempData["Success"] = "Unenrolled successfully.";
        return RedirectToAction("Index");
    }
}
