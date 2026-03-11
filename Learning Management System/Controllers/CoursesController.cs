using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CoursesController(ICourseService courseService) : ControllerBase
{
    /// <summary>GET api/courses</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IList<CourseDto>>>> GetAll()
    {
        var courses = await courseService.GetAllAsync();
        return Ok(ApiResponse<IList<CourseDto>>.Ok(courses));
    }

    /// <summary>GET api/courses/{id}</summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<CourseDetailDto>>> GetById(int id)
    {
        var course = await courseService.GetByIdAsync(id);
        return Ok(ApiResponse<CourseDetailDto>.Ok(course));
    }

    /// <summary>POST api/courses</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourseDto>>> Create(CreateCourseRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var course = await courseService.CreateAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = course.Id },
            ApiResponse<CourseDto>.Ok(course, "Course created."));
    }

    /// <summary>PUT api/courses/{id}</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> Update(int id, UpdateCourseRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var course = await courseService.UpdateAsync(id, userId, request);
        return Ok(ApiResponse<CourseDto>.Ok(course, "Course updated."));
    }

    /// <summary>DELETE api/courses/{id}</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await courseService.DeleteAsync(id, userId);
        return Ok(ApiResponse.Ok("Course deleted."));
    }

    /// <summary>POST api/courses/{id}/enroll</summary>
    [Authorize(Roles = "Student")]
    [HttpPost("{id:int}/enroll")]
    public async Task<ActionResult<ApiResponse>> Enroll(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await courseService.EnrollStudentAsync(id, userId);
        return Ok(ApiResponse.Ok("Enrolled successfully."));
    }

    /// <summary>DELETE api/courses/{id}/enroll</summary>
    [Authorize(Roles = "Student")]
    [HttpDelete("{id:int}/enroll")]
    public async Task<ActionResult<ApiResponse>> Unenroll(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await courseService.UnenrollStudentAsync(id, userId);
        return Ok(ApiResponse.Ok("Unenrolled successfully."));
    }

    /// <summary>GET api/courses/my-courses (instructor)</summary>
    [Authorize(Roles = "Instructor")]
    [HttpGet("my-courses")]
    public async Task<ActionResult<ApiResponse<IList<CourseDto>>>> GetMyCourses()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var courses = await courseService.GetByInstructorAsync(userId);
        return Ok(ApiResponse<IList<CourseDto>>.Ok(courses));
    }

    /// <summary>GET api/courses/enrolled (student)</summary>
    [Authorize(Roles = "Student")]
    [HttpGet("enrolled")]
    public async Task<ActionResult<ApiResponse<IList<CourseDto>>>> GetEnrolled()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var courses = await courseService.GetEnrolledCoursesAsync(userId);
        return Ok(ApiResponse<IList<CourseDto>>.Ok(courses));
    }
}
