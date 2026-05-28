using LMS.Data;
using LMS.DTOs;
using LMS.Extensions;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class SubmissionService(ApplicationDbContext db, IFileStorageService fileStorage) : ISubmissionService
{
    // Upsert flow: validate enrollment/deadline → find existing submission → update or create.
    // Uses <= for deadline check so submissions at the exact deadline second are rejected.
    public async Task<ServiceResult<SubmissionDto>> SubmitAsync(int assignmentId, string studentId, CreateSubmissionRequest request, string? s3Key = null, string? originalFileName = null)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment is null)
            return ServiceResult.Failure<SubmissionDto>(ErrorType.NotFound, "Assignment not found.");

        var enrolled = await db.Enrollments
            .AnyAsync(e => e.CourseId == assignment.CourseId && e.StudentId == studentId);
        if (!enrolled)
            return ServiceResult.Failure<SubmissionDto>(ErrorType.Unauthorized, "Please enroll in this course before accessing or submitting assignments.");

        if (string.IsNullOrWhiteSpace(request.TextContent) && string.IsNullOrWhiteSpace(s3Key))
            return ServiceResult.Failure<SubmissionDto>(ErrorType.ValidationError, "Submission must include text content or a file.");

        if (assignment.DueDate <= DateTime.UtcNow)
            return ServiceResult.Failure<SubmissionDto>(ErrorType.Conflict, "Submission is locked — the deadline has passed.");

        var existing = await db.Submissions
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

        if (existing is not null)
        {
            if (existing.Grade is not null)
                return ServiceResult.Failure<SubmissionDto>(ErrorType.Conflict, "Submission is locked — it has already been graded.");

            // Delete old file from S3 before overwriting to avoid orphaned objects
            if (!string.IsNullOrWhiteSpace(existing.FilePath))
                await fileStorage.DeleteAsync(existing.FilePath);

            existing.TextContent = request.TextContent;
            existing.FilePath = s3Key;
            existing.OriginalFileName = originalFileName;
            existing.SubmittedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var updatedDto = await QuerySubmissions().FirstAsync(s => s.Id == existing.Id);
            return ServiceResult.Success(updatedDto);
        }

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            TextContent = request.TextContent,
            FilePath = s3Key,
            OriginalFileName = originalFileName
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

    public async Task<ServiceResult<string>> GetDownloadUrlAsync(int submissionId, string requestingUserId, bool isInstructorOrAdmin)
    {
        var submission = await db.Submissions.FirstOrDefaultAsync(s => s.Id == submissionId);
        if (submission is null)
            return ServiceResult.Failure<string>(ErrorType.NotFound, "Submission not found.");

        if (!isInstructorOrAdmin && submission.StudentId != requestingUserId)
            return ServiceResult.Failure<string>(ErrorType.Unauthorized, "You can only download your own submissions.");

        if (string.IsNullOrWhiteSpace(submission.FilePath) || string.IsNullOrWhiteSpace(submission.OriginalFileName))
            return ServiceResult.Failure<string>(ErrorType.NotFound, "This submission has no attached file.");

        var url = fileStorage.GetDownloadUrl(submission.FilePath, submission.OriginalFileName);
        return ServiceResult.Success(url);
    }

    // Returns raw DTO (not ServiceResult) for internal controller use when populating view data
    public async Task<SubmissionDto?> GetByStudentForAssignmentAsync(int assignmentId, string studentId)
    {
        return await QuerySubmissions()
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);
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
                OriginalFileName = s.OriginalFileName,
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
