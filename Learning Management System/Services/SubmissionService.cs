using LMS.Data;
using LMS.DTOs;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class SubmissionService(ApplicationDbContext db) : ISubmissionService
{
    public async Task<SubmissionDto> SubmitAsync(int assignmentId, string studentId, CreateSubmissionRequest request)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == assignmentId)
            ?? throw new KeyNotFoundException("Assignment not found.");

        // Verify student is enrolled in the course
        var enrolled = await db.Enrollments
            .AnyAsync(e => e.CourseId == assignment.CourseId && e.StudentId == studentId);
        if (!enrolled)
            throw new UnauthorizedAccessException("You must be enrolled in the course to submit assignments.");

        // Check for existing submission
        var existing = await db.Submissions
            .AnyAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);
        if (existing)
            throw new InvalidOperationException("You have already submitted this assignment.");

        if (string.IsNullOrWhiteSpace(request.TextContent) && string.IsNullOrWhiteSpace(request.FilePath))
            throw new ArgumentException("Submission must include text content or a file path.");

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            TextContent = request.TextContent,
            FilePath = request.FilePath
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        return await MapToDto(submission.Id);
    }

    public async Task<SubmissionDto> GetByIdAsync(int id)
    {
        return await QuerySubmissions()
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Submission not found.");
    }

    public async Task<IList<SubmissionDto>> GetByAssignmentAsync(int assignmentId)
    {
        var exists = await db.Assignments.AnyAsync(a => a.Id == assignmentId);
        if (!exists) throw new KeyNotFoundException("Assignment not found.");

        return await QuerySubmissions()
            .Where(s => s.AssignmentId == assignmentId)
            .ToListAsync();
    }

    public async Task<IList<SubmissionDto>> GetByStudentAsync(string studentId)
    {
        return await QuerySubmissions()
            .Where(s => s.StudentId == studentId)
            .ToListAsync();
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

    private async Task<SubmissionDto> MapToDto(int submissionId)
    {
        return await QuerySubmissions().FirstAsync(s => s.Id == submissionId);
    }
}
