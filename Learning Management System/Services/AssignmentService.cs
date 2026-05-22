using LMS.Data;
using LMS.DTOs;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

// Handles assignment CRUD with ownership checks against the parent course's instructor
public class AssignmentService(ApplicationDbContext db) : IAssignmentService
{
    // Verifies the caller owns the course (or is admin) before creating the assignment
    public async Task<AssignmentDto> CreateAsync(int courseId, string userId, bool isAdmin, CreateAssignmentRequest request)
    {
        var course = await db.Courses.FindAsync(courseId)
            ?? throw new KeyNotFoundException("Course not found.");

        if (!isAdmin && course.InstructorId != userId)
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

        return await QueryAssignments().FirstAsync(a => a.Id == assignment.Id);
    }

    public async Task<AssignmentDto> GetByIdAsync(int id)
    {
        return await QueryAssignments().FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");
    }

    public async Task<IList<AssignmentDto>> GetByCourseAsync(int courseId)
    {
        var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists) throw new KeyNotFoundException("Course not found.");

        return await QueryAssignments()
            .Where(a => a.CourseId == courseId)
            .ToListAsync();
    }

    // Includes the parent course to check instructor ownership before allowing the update
    public async Task<AssignmentDto> UpdateAsync(int id, string userId, bool isAdmin, UpdateAssignmentRequest request)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        if (!isAdmin && assignment.Course.InstructorId != userId)
            throw new UnauthorizedAccessException("You can only update assignments for your own courses.");

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.DueDate = request.DueDate;
        await db.SaveChangesAsync();

        return await QueryAssignments().FirstAsync(a => a.Id == id);
    }

    public async Task DeleteAsync(int id, string userId, bool isAdmin)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException("Assignment not found.");

        if (!isAdmin && assignment.Course.InstructorId != userId)
            throw new UnauthorizedAccessException("You can only delete assignments for your own courses.");

        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync();
    }

    // Reusable projection query — keeps AssignmentDto mapping in one place
    private IQueryable<AssignmentDto> QueryAssignments()
    {
        return db.Assignments
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
            });
    }
}
