using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

public class CourseController(ICourseService courseService) : BaseController
{
    public async Task<IActionResult> Index(int page = 1, int pageSize = 9)
    {
        var result = await courseService.GetAllAsync(page, pageSize);
        return HandleResult(result, courses => View(courses));
    }

    public async Task<IActionResult> Details(int id)
    {
        var result = await courseService.GetByIdAsync(id);
        return HandleResult(result, course => View(course));
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
        var result = await courseService.CreateAsync(userId, request);
        return HandleResult(result, course =>
        {
            TempData["Success"] = "Course created successfully.";
            return RedirectToAction("Details", new { id = course.Id });
        });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await courseService.GetByIdAsync(id);
        return HandleResult(result, course =>
        {
            var request = new UpdateCourseRequest
            {
                Title = course.Title,
                Description = course.Description
            };
            ViewBag.CourseId = id;
            return View(request);
        });
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
        var result = await courseService.UpdateAsync(id, userId, User.IsInRole("Admin"), request);
        return HandleResult(result, _ =>
        {
            TempData["Success"] = "Course updated successfully.";
            return RedirectToAction("Details", new { id });
        });
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await courseService.GetByIdAsync(id);
        return HandleResult(result, course => View(course));
    }

    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await courseService.DeleteAsync(id, userId, User.IsInRole("Admin"));
        return HandleResult(result, () =>
        {
            TempData["Success"] = "Course deleted successfully.";
            return RedirectToAction("Index");
        });
    }

    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> MyCourses(int page = 1, int pageSize = 9)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await courseService.GetByInstructorAsync(userId, page, pageSize);
        return HandleResult(result, courses => View(courses));
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Enrolled(int page = 1, int pageSize = 9)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await courseService.GetEnrolledCoursesAsync(userId, page, pageSize);
        return HandleResult(result, courses => View(courses));
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await courseService.EnrollStudentAsync(id, userId);
        return HandleResultWithFeedback(result,
            () =>
            {
                TempData["Success"] = "Enrolled successfully.";
                return RedirectToAction("Details", new { id });
            },
            () => RedirectToAction("Details", new { id }));
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unenroll(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await courseService.UnenrollStudentAsync(id, userId);
        return HandleResult(result, () =>
        {
            TempData["Success"] = "Unenrolled successfully.";
            return RedirectToAction("Index");
        });
    }
}
