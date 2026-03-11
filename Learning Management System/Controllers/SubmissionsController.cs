using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

[ApiController]
[Route("api/assignments/{assignmentId:int}/submissions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SubmissionsController(ISubmissionService submissionService) : ControllerBase
{
    /// <summary>POST api/assignments/{assignmentId}/submissions</summary>
    [Authorize(Roles = "Student")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubmissionDto>>> Submit(int assignmentId, CreateSubmissionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var submission = await submissionService.SubmitAsync(assignmentId, userId, request);
        return CreatedAtAction(nameof(GetById), new { assignmentId, id = submission.Id },
            ApiResponse<SubmissionDto>.Ok(submission, "Assignment submitted."));
    }

    /// <summary>GET api/assignments/{assignmentId}/submissions</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IList<SubmissionDto>>>> GetByAssignment(int assignmentId)
    {
        var submissions = await submissionService.GetByAssignmentAsync(assignmentId);
        return Ok(ApiResponse<IList<SubmissionDto>>.Ok(submissions));
    }

    /// <summary>GET api/assignments/{assignmentId}/submissions/{id}</summary>
    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SubmissionDto>>> GetById(int assignmentId, int id)
    {
        var submission = await submissionService.GetByIdAsync(id);
        return Ok(ApiResponse<SubmissionDto>.Ok(submission));
    }

    /// <summary>GET api/assignments/my-submissions (student)</summary>
    [Authorize(Roles = "Student")]
    [HttpGet("/api/submissions/mine")]
    public async Task<ActionResult<ApiResponse<IList<SubmissionDto>>>> GetMySubmissions()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var submissions = await submissionService.GetByStudentAsync(userId);
        return Ok(ApiResponse<IList<SubmissionDto>>.Ok(submissions));
    }
}
