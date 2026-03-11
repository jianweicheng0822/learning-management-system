using LMS.Data;
using LMS.DTOs;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class AssignmentService(ApplicationDbContext db) : IAssignmentService
{
    public async Task<AssignmentDto> CreateAsync(int courseId, string instructorId, CreateAssignmentRequest request)
    {
        var course = await db.Courses.FindAsync(courseId)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You can only create assignments for your own courses.");

        var assignment = new Assignment
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            CourseId = courseId
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        return await MapToDto(assignment.Id);
    }

    public async Task<AssignmentDto> GetByIdAsync(int id)
    {
        return await db.Assignments
            .Where(a => a.Id == id)
            .Include(a => a.Course)
            .Include(a => a.Submissions)
            .Select(a => new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                CourseId = a.CourseId,
                CourseName = a.Course.Title,
                SubmissionCount = a.Submissions.Count,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Assignment not found.");
    }

    public async Task<IList<AssignmentDto>> GetByCourseAsync(int courseId)
    {
        var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists) throw new KeyNotFoundException("Course not found.");

        return await db.Assignments
            .Where(a => a.CourseId == courseId)
            .Include(a => a.Course)
            .Include(a => a.Submissions)
            .Select(a => new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                CourseId = a.CourseId,
                CourseName = a.Course.Title,
                SubmissionCount = a.Submissions.Count,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AssignmentDto> UpdateAsync(int id, string instructorId, UpdateAssignmentRequest request)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        if (assignment.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You can only update assignments for your own courses.");

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.DueDate = request.DueDate;
        await db.SaveChangesAsync();

        return await MapToDto(id);
    }

    public async Task DeleteAsync(int id, string instructorId)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        if (assignment.Course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You can only delete assignments for your own courses.");

        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync();
    }

    private async Task<AssignmentDto> MapToDto(int assignmentId)
    {
        return await db.Assignments
            .Where(a => a.Id == assignmentId)
            .Include(a => a.Course)
            .Include(a => a.Submissions)
            .Select(a => new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                CourseId = a.CourseId,
                CourseName = a.Course.Title,
                SubmissionCount = a.Submissions.Count,
                CreatedAt = a.CreatedAt
            })
            .FirstAsync();
    }
}
