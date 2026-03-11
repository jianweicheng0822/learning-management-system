using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/assignments")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AssignmentsController(IAssignmentService assignmentService) : ControllerBase
{
    /// <summary>GET api/courses/{courseId}/assignments</summary>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IList<AssignmentDto>>>> GetByCourse(int courseId)
    {
        var assignments = await assignmentService.GetByCourseAsync(courseId);
        return Ok(ApiResponse<IList<AssignmentDto>>.Ok(assignments));
    }

    /// <summary>GET api/courses/{courseId}/assignments/{id}</summary>
    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> GetById(int courseId, int id)
    {
        var assignment = await assignmentService.GetByIdAsync(id);
        return Ok(ApiResponse<AssignmentDto>.Ok(assignment));
    }

    /// <summary>POST api/courses/{courseId}/assignments</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> Create(int courseId, CreateAssignmentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var assignment = await assignmentService.CreateAsync(courseId, userId, request);
        return CreatedAtAction(nameof(GetById), new { courseId, id = assignment.Id },
            ApiResponse<AssignmentDto>.Ok(assignment, "Assignment created."));
    }

    /// <summary>PUT api/courses/{courseId}/assignments/{id}</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<AssignmentDto>>> Update(int courseId, int id, UpdateAssignmentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var assignment = await assignmentService.UpdateAsync(id, userId, request);
        return Ok(ApiResponse<AssignmentDto>.Ok(assignment, "Assignment updated."));
    }

    /// <summary>DELETE api/courses/{courseId}/assignments/{id}</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> Delete(int courseId, int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await assignmentService.DeleteAsync(id, userId);
        return Ok(ApiResponse.Ok("Assignment deleted."));
    }
}
