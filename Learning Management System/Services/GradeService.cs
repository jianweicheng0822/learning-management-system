using LMS.Data;
using LMS.DTOs;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class GradeService(ApplicationDbContext db) : IGradeService
{
    public async Task<GradeDto> GradeSubmissionAsync(int submissionId, string instructorId, GradeSubmissionRequest request)
    {
        var submission = await db.Submissions
            .Include(s => s.Assignment).ThenInclude(a => a.Course)
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.Id == submissionId)
            ?? throw new KeyNotFoundException("Submission not found.");

        if (submission.Assignment.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You can only grade submissions for your own courses.");

        if (submission.Grade is not null)
            throw new InvalidOperationException("This submission has already been graded. Use the update endpoint.");

        var grade = new Grade
        {
            SubmissionId = submissionId,
            Score = request.Score,
            Feedback = request.Feedback,
            GradedById = instructorId
        };

        db.Grades.Add(grade);
        await db.SaveChangesAsync();

        return await MapToDto(grade.Id);
    }

    public async Task<GradeDto> UpdateGradeAsync(int submissionId, string instructorId, GradeSubmissionRequest request)
    {
        var grade = await db.Grades
            .Include(g => g.Submission).ThenInclude(s => s.Assignment).ThenInclude(a => a.Course)
            .FirstOrDefaultAsync(g => g.SubmissionId == submissionId)
            ?? throw new KeyNotFoundException("Grade not found for this submission.");

        if (grade.Submission.Assignment.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You can only update grades for your own courses.");

        grade.Score = request.Score;
        grade.Feedback = request.Feedback;
        grade.GradedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await MapToDto(grade.Id);
    }

    public async Task<IList<SubmissionDto>> GetGradesForStudentCourseAsync(string studentId, int courseId)
    {
        return await db.Submissions
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
            .ToListAsync();
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
