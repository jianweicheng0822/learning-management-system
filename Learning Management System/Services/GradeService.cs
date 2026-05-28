using LMS.Data;
using LMS.DTOs;
using LMS.Extensions;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class GradeService(ApplicationDbContext db) : IGradeService
{
    public async Task<ServiceResult<GradeDto>> GradeSubmissionAsync(int submissionId, string userId, bool isAdmin, GradeSubmissionRequest request)
    {
        var submission = await db.Submissions
            .Include(s => s.Assignment).ThenInclude(a => a.Course)
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (submission is null)
            return ServiceResult.Failure<GradeDto>(ErrorType.NotFound, "Submission not found.");

        if (!isAdmin && submission.Assignment.Course.InstructorId != userId)
            return ServiceResult.Failure<GradeDto>(ErrorType.Unauthorized, "You can only grade submissions for your own courses.");

        if (submission.Grade is not null)
            return ServiceResult.Failure<GradeDto>(ErrorType.Conflict, "This submission has already been graded. Use the update endpoint.");

        var grade = new Grade
        {
            SubmissionId = submissionId,
            Score = request.Score,
            Feedback = request.Feedback,
            GradedById = userId
        };

        db.Grades.Add(grade);
        await db.SaveChangesAsync();

        var dto = await MapToDto(grade.Id);
        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult<GradeDto>> UpdateGradeAsync(int submissionId, string userId, bool isAdmin, GradeSubmissionRequest request)
    {
        var grade = await db.Grades
            .Include(g => g.Submission).ThenInclude(s => s.Assignment).ThenInclude(a => a.Course)
            .FirstOrDefaultAsync(g => g.SubmissionId == submissionId);

        if (grade is null)
            return ServiceResult.Failure<GradeDto>(ErrorType.NotFound, "Grade not found for this submission.");

        if (!isAdmin && grade.Submission.Assignment.Course.InstructorId != userId)
            return ServiceResult.Failure<GradeDto>(ErrorType.Unauthorized, "You can only update grades for your own courses.");

        grade.Score = request.Score;
        grade.Feedback = request.Feedback;
        grade.GradedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var dto = await MapToDto(grade.Id);
        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult<PagedResult<SubmissionDto>>> GetGradesForStudentCourseAsync(string studentId, int courseId, int page = 1, int pageSize = 10)
    {
        var paged = await db.Submissions
            .Where(s => s.StudentId == studentId && s.Assignment.CourseId == courseId)
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
            })
            .ToPagedResultAsync(page, pageSize);

        return ServiceResult.Success(paged);
    }

    // Only includes graded submissions; returns null if no submissions have been graded yet
    public async Task<decimal?> GetAverageScoreAsync(string studentId, int courseId)
    {
        var scores = await db.Submissions
            .Where(s => s.StudentId == studentId && s.Assignment.CourseId == courseId && s.Grade != null)
            .Select(s => s.Grade!.Score)
            .ToListAsync();

        return scores.Count > 0 ? scores.Average() : null;
    }

    private async Task<GradeDto> MapToDto(int gradeId)
    {
        return await db.Grades
            .Where(g => g.Id == gradeId)
            .Include(g => g.GradedBy)
            .Select(g => new GradeDto
            {
                Id = g.Id,
                Score = g.Score,
                Feedback = g.Feedback,
                GradedAt = g.GradedAt,
                GradedByName = g.GradedBy.FullName
            })
            .FirstAsync();
    }
}
