using LMS.Data;
using LMS.DTOs;
using LMS.Extensions;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class CourseService(ApplicationDbContext db) : ICourseService
{
    public async Task<ServiceResult<CourseDto>> CreateAsync(string instructorId, CreateCourseRequest request)
    {
        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            InstructorId = instructorId
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var dto = await QueryCourses().FirstAsync(c => c.Id == course.Id);
        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult<PagedResult<CourseDto>>> GetAllAsync(int page = 1, int pageSize = 9)
    {
        var paged = await QueryCourses().ToPagedResultAsync(page, pageSize);
        return ServiceResult.Success(paged);
    }

    public async Task<ServiceResult<CourseDetailDto>> GetByIdAsync(int id)
    {
        var course = await db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments).ThenInclude(e => e.Student)
            .Include(c => c.Assignments).ThenInclude(a => a.Submissions)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null)
            return ServiceResult.Failure<CourseDetailDto>(ErrorType.NotFound, "Course not found.");

        var dto = new CourseDetailDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor.FullName,
            EnrolledCount = course.Enrollments.Count,
            CreatedAt = course.CreatedAt,
            EnrolledStudents = course.Enrollments.Select(e => new EnrolledStudentDto
            {
                StudentId = e.StudentId,
                FullName = e.Student.FullName,
                Email = e.Student.Email!,
                EnrolledAt = e.EnrolledAt
            }).ToList(),
            Assignments = course.Assignments.Select(a => new AssignmentDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                CourseId = a.CourseId,
                CourseName = course.Title,
                SubmissionCount = a.Submissions.Count,
                CreatedAt = a.CreatedAt
            }).ToList()
        };

        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult<CourseDto>> UpdateAsync(int id, string userId, bool isAdmin, UpdateCourseRequest request)
    {
        var course = await db.Courses.FindAsync(id);
        if (course is null)
            return ServiceResult.Failure<CourseDto>(ErrorType.NotFound, "Course not found.");

        if (!isAdmin && course.InstructorId != userId)
            return ServiceResult.Failure<CourseDto>(ErrorType.Unauthorized, "You can only update your own courses.");

        course.Title = request.Title;
        course.Description = request.Description;
        await db.SaveChangesAsync();

        var dto = await QueryCourses().FirstAsync(c => c.Id == id);
        return ServiceResult.Success(dto);
    }

    public async Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin)
    {
        var course = await db.Courses.FindAsync(id);
        if (course is null)
            return ServiceResult.Failure(ErrorType.NotFound, "Course not found.");

        if (!isAdmin && course.InstructorId != userId)
            return ServiceResult.Failure(ErrorType.Unauthorized, "You can only delete your own courses.");

        db.Courses.Remove(course);
        await db.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> EnrollStudentAsync(int courseId, string studentId)
    {
        var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists)
            return ServiceResult.Failure(ErrorType.NotFound, "Course not found.");

        var alreadyEnrolled = await db.Enrollments
            .AnyAsync(e => e.CourseId == courseId && e.StudentId == studentId);
        if (alreadyEnrolled)
            return ServiceResult.Failure(ErrorType.Conflict, "Already enrolled in this course.");

        db.Enrollments.Add(new Enrollment
        {
            CourseId = courseId,
            StudentId = studentId
        });

        await db.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UnenrollStudentAsync(int courseId, string studentId)
    {
        var enrollment = await db.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.StudentId == studentId);

        if (enrollment is null)
            return ServiceResult.Failure(ErrorType.NotFound, "Enrollment not found.");

        db.Enrollments.Remove(enrollment);
        await db.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<PagedResult<CourseDto>>> GetByInstructorAsync(string instructorId, int page = 1, int pageSize = 9)
    {
        var paged = await QueryCourses()
            .Where(c => c.InstructorId == instructorId)
            .ToPagedResultAsync(page, pageSize);
        return ServiceResult.Success(paged);
    }

    // Subquery approach: get enrolled course IDs first, then filter via Contains.
    // This avoids a join which would duplicate the QueryCourses() projection logic.
    public async Task<ServiceResult<PagedResult<CourseDto>>> GetEnrolledCoursesAsync(string studentId, int page = 1, int pageSize = 9)
    {
        var enrolledCourseIds = db.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => e.CourseId);

        var paged = await QueryCourses()
            .Where(c => enrolledCourseIds.Contains(c.Id))
            .ToPagedResultAsync(page, pageSize);

        return ServiceResult.Success(paged);
    }

    private IQueryable<CourseDto> QueryCourses()
    {
        return db.Courses
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor.FullName,
                EnrolledCount = c.Enrollments.Count,
                CreatedAt = c.CreatedAt
            });
    }
}
