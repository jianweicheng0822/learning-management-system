using LMS.Data;
using LMS.DTOs;
using LMS.Extensions;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class AssignmentService(ApplicationDbContext db) : IAssignmentService
{
    public async Task<ServiceResult<AssignmentDto>> CreateAsync(int courseId, string userId, bool isAdmin, CreateAssignmentRequest request)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course is null)
            return ServiceResult.Failure<AssignmentDto>(ErrorType.NotFound, "Course not found.");

        if (!isAdmin && course.InstructorId != userId)
            return ServiceResult.Failure<AssignmentDto>(ErrorType.Unauthorized, "You can only create assignments for your own courses.");

        var assignment = new Assignment
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            CourseId = courseId
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        var dto = await QueryAssignments().FirstAsync(a => a.Id == assignment.Id);
        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult<AssignmentDto>> GetByIdAsync(int id)
    {
        var dto = await QueryAssignments().FirstOrDefaultAsync(a => a.Id == id);
        if (dto is null)
            return ServiceResult.Failure<AssignmentDto>(ErrorType.NotFound, "Assignment not found.");

        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult<PagedResult<AssignmentDto>>> GetByCourseAsync(int courseId, int page = 1, int pageSize = 10)
    {
        var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
            return ServiceResult.Failure<PagedResult<AssignmentDto>>(ErrorType.NotFound, "Course not found.");

        var paged = await QueryAssignments()
            .Where(a => a.CourseId == courseId)
            .ToPagedResultAsync(page, pageSize);

        return ServiceResult.Success(paged);
    }

    public async Task<ServiceResult<AssignmentDto>> UpdateAsync(int id, string userId, bool isAdmin, UpdateAssignmentRequest request)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);
        if (assignment is null)
            return ServiceResult.Failure<AssignmentDto>(ErrorType.NotFound, "Assignment not found.");

        if (!isAdmin && assignment.Course.InstructorId != userId)
            return ServiceResult.Failure<AssignmentDto>(ErrorType.Unauthorized, "You can only update assignments for your own courses.");

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.DueDate = request.DueDate;
        await db.SaveChangesAsync();

        var dto = await QueryAssignments().FirstAsync(a => a.Id == id);
        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin)
    {
        var assignment = await db.Assignments.Include(a => a.Course).FirstOrDefaultAsync(a => a.Id == id);
        if (assignment is null)
            return ServiceResult.Failure(ErrorType.NotFound, "Assignment not found.");

        if (!isAdmin && assignment.Course.InstructorId != userId)
            return ServiceResult.Failure(ErrorType.Unauthorized, "You can only delete assignments for your own courses.");

        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync();
        return ServiceResult.Success();
    }

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
