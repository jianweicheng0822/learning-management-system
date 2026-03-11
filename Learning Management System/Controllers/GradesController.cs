using System.Security.Claims;
using LMS.DTOs;
using LMS.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers;

[ApiController]
[Route("api/submissions/{submissionId:int}/grade")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class GradesController(IGradeService gradeService) : ControllerBase
{
    /// <summary>POST api/submissions/{submissionId}/grade</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<GradeDto>>> Grade(int submissionId, GradeSubmissionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var grade = await gradeService.GradeSubmissionAsync(submissionId, userId, request);
        return Ok(ApiResponse<GradeDto>.Ok(grade, "Submission graded."));
    }

    /// <summary>PUT api/submissions/{submissionId}/grade</summary>
    [Authorize(Roles = "Instructor,Admin")]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<GradeDto>>> UpdateGrade(int submissionId, GradeSubmissionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var grade = await gradeService.UpdateGradeAsync(submissionId, userId, request);
        return Ok(ApiResponse<GradeDto>.Ok(grade, "Grade updated."));
    }

    /// <summary>GET api/courses/{courseId}/grades (student view)</summary>
    [Authorize(Roles = "Student")]
    [HttpGet("/api/courses/{courseId:int}/grades")]
    public async Task<ActionResult<ApiResponse<IList<SubmissionDto>>>> GetMyGrades(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var grades = await gradeService.GetGradesForStudentCourseAsync(userId, courseId);
        return Ok(ApiResponse<IList<SubmissionDto>>.Ok(grades));
    }
}
