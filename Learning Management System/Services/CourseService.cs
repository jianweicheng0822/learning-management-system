using LMS.Data;
using LMS.DTOs;
using LMS.Models;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services;

public class CourseService(ApplicationDbContext db) : ICourseService
{
    public async Task<CourseDto> CreateAsync(string instructorId, CreateCourseRequest request)
    {
        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            InstructorId = instructorId
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync();

        return await MapToDto(course.Id);
    }

    public async Task<IList<CourseDto>> GetAllAsync()
    {
        return await db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor.FullName,
                EnrolledCount = c.Enrollments.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CourseDetailDto> GetByIdAsync(int id)
    {
        var course = await db.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments).ThenInclude(e => e.Student)
            .Include(c => c.Assignments).ThenInclude(a => a.Submissions)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException("Course not found.");

        return new CourseDetailDto
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
    }

    public async Task<CourseDto> UpdateAsync(int id, string instructorId, UpdateCourseRequest request)
    {
        var course = await db.Courses.FindAsync(id)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You can only update your own courses.");

        course.Title = request.Title;
        course.Description = request.Description;
        await db.SaveChangesAsync();

        return await MapToDto(id);
    }

    public async Task DeleteAsync(int id, string instructorId)
    {
        var course = await db.Courses.FindAsync(id)
            ?? throw new KeyNotFoundException("Course not found.");

        if (course.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You can only delete your own courses.");

        db.Courses.Remove(course);
        await db.SaveChangesAsync();
    }

    public async Task<IList<CourseDto>> GetByInstructorAsync(string instructorId)
    {
        return await db.Courses
            .Where(c => c.InstructorId == instructorId)
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor.FullName,
                EnrolledCount = c.Enrollments.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    private async Task<CourseDto> MapToDto(int courseId)
    {
        return await db.Courses
            .Where(c => c.Id == courseId)
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                InstructorName = c.Instructor.FullName,
                EnrolledCount = c.Enrollments.Count,
                CreatedAt = c.CreatedAt
            })
            .FirstAsync();
    }
}
