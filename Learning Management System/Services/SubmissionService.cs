using LMS.Data;
using LMS.DTOs;
using LMS.Extensions;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class SubmissionService(ApplicationDbContext db) : ISubmissionService
{
    public async Task<ServiceResult<SubmissionDto>> SubmitAsync(int assignmentId, string studentId, CreateSubmissionRequest request)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment is null)
            return ServiceResult.Failure<SubmissionDto>(ErrorType.NotFound, "Assignment not found.");

        var enrolled = await db.Enrollments
            .AnyAsync(e => e.CourseId == assignment.CourseId && e.StudentId == studentId);
        if (!enrolled)
            return ServiceResult.Failure<SubmissionDto>(ErrorType.Unauthorized, "You must be enrolled in the course to submit assignments.");

        var existing = await db.Submissions
            .AnyAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);
        if (existing)
            return ServiceResult.Failure<SubmissionDto>(ErrorType.Conflict, "You have already submitted this assignment.");

        if (string.IsNullOrWhiteSpace(request.TextContent) && string.IsNullOrWhiteSpace(request.FilePath))
            return ServiceResult.Failure<SubmissionDto>(ErrorType.ValidationError, "Submission must include text content or a file path.");

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            TextContent = request.TextContent,
            FilePath = request.FilePath
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        var dto = await QuerySubmissions().FirstAsync(s => s.Id == submission.Id);
        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult<SubmissionDto>> GetByIdAsync(int id)
    {
        var dto = await QuerySubmissions().FirstOrDefaultAsync(s => s.Id == id);
        if (dto is null)
            return ServiceResult.Failure<SubmissionDto>(ErrorType.NotFound, "Submission not found.");

        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult<PagedResult<SubmissionDto>>> GetByAssignmentAsync(int assignmentId, int page = 1, int pageSize = 10)
    {
        var exists = await db.Assignments.AnyAsync(a => a.Id == assignmentId);
        if (!exists)
            return ServiceResult.Failure<PagedResult<SubmissionDto>>(ErrorType.NotFound, "Assignment not found.");

        var paged = await QuerySubmissions()
            .Where(s => s.AssignmentId == assignmentId)
            .ToPagedResultAsync(page, pageSize);

        return ServiceResult.Success(paged);
    }

    public async Task<ServiceResult<PagedResult<SubmissionDto>>> GetByStudentAsync(string studentId, int page = 1, int pageSize = 10)
    {
        var paged = await QuerySubmissions()
            .Where(s => s.StudentId == studentId)
            .ToPagedResultAsync(page, pageSize);

        return ServiceResult.Success(paged);
    }

    private IQueryable<SubmissionDto> QuerySubmissions()
    {
        return db.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .Include(s => s.Grade).ThenInclude(g => g!.GradedBy)
            .Select(s => new SubmissionDto
            {
                Id = s.Id,
                TextContent = s.TextContent,
                FilePath = s.FilePath,
                SubmittedAt = s.SubmittedAt,
                StudentId = s.StudentId,
                StudentName = s.Student.FullName,
                AssignmentId = s.AssignmentId,
                AssignmentTitle = s.Assignment.Title,
                Grade = s.Grade == null ? null : new GradeDto
                {
                    Id = s.Grade.Id,
                    Score = s.Grade.Score,
                    Feedback = s.Grade.Feedback,
                    GradedAt = s.Grade.GradedAt,
                    GradedByName = s.Grade.GradedBy.FullName
                }
            });
    }
}
