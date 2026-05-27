using LMS.Data;
using LMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Tests;

public class TestDbHelper : IDisposable
{
    private readonly SqliteConnection _connection;
    public ApplicationDbContext Context { get; }

    public TestDbHelper()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public ApplicationUser CreateUser(string id, string fullName, string email)
    {
        var user = new ApplicationUser
        {
            Id = id,
            UserName = email,
            Email = email,
            FullName = fullName,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = email.ToUpper()
        };
        Context.Users.Add(user);
        Context.SaveChanges();
        return user;
    }

    public Course CreateCourse(string instructorId, string title = "Test Course")
    {
        var course = new Course
        {
            Title = title,
            Description = "Test Description",
            InstructorId = instructorId
        };
        Context.Courses.Add(course);
        Context.SaveChanges();
        return course;
    }

    public Enrollment CreateEnrollment(string studentId, int courseId)
    {
        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId
        };
        Context.Enrollments.Add(enrollment);
        Context.SaveChanges();
        return enrollment;
    }

    public Assignment CreateAssignment(int courseId, string title = "Test Assignment")
    {
        var assignment = new Assignment
        {
            Title = title,
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            CourseId = courseId
        };
        Context.Assignments.Add(assignment);
        Context.SaveChanges();
        return assignment;
    }

    public Submission CreateSubmission(string studentId, int assignmentId, string? filePath = null, string? originalFileName = null)
    {
        var submission = new Submission
        {
            StudentId = studentId,
            AssignmentId = assignmentId,
            TextContent = "Test submission content",
            FilePath = filePath,
            OriginalFileName = originalFileName
        };
        Context.Submissions.Add(submission);
        Context.SaveChanges();
        return submission;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
