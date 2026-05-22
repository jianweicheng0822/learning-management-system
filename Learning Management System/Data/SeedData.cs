using LMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Idempotent: skip if any users already exist
        if (await userManager.Users.AnyAsync())
            return;

        var db = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // --- Create demo users ---
        var admin = new ApplicationUser
        {
            UserName = "admin@lms.com",
            Email = "admin@lms.com",
            FullName = "System Admin",
            EmailConfirmed = true
        };

        var instructor = new ApplicationUser
        {
            UserName = "instructor@lms.com",
            Email = "instructor@lms.com",
            FullName = "Dr. Jane Smith",
            EmailConfirmed = true
        };

        var student = new ApplicationUser
        {
            UserName = "student@lms.com",
            Email = "student@lms.com",
            FullName = "Alex Johnson",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");

        await userManager.CreateAsync(instructor, "Teach123!");
        await userManager.AddToRoleAsync(instructor, "Instructor");

        await userManager.CreateAsync(student, "Learn123!");
        await userManager.AddToRoleAsync(student, "Student");

        // --- Create courses ---
        var courses = new[]
        {
            new Course
            {
                Title = "Introduction to Computer Science",
                Description = "Covers fundamental concepts of programming, algorithms, and data structures. Perfect for beginners looking to build a strong foundation in CS.",
                InstructorId = instructor.Id
            },
            new Course
            {
                Title = "Web Development with ASP.NET",
                Description = "Learn to build modern web applications using ASP.NET Core MVC, Entity Framework, and Identity. Includes hands-on projects.",
                InstructorId = instructor.Id
            },
            new Course
            {
                Title = "Database Systems",
                Description = "Study relational database design, SQL, normalization, and query optimization. Covers both theory and practical applications with MySQL.",
                InstructorId = instructor.Id
            }
        };

        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();

        // --- Enroll student in all courses ---
        var enrollments = courses.Select(c => new Enrollment
        {
            StudentId = student.Id,
            CourseId = c.Id
        });

        db.Enrollments.AddRange(enrollments);
        await db.SaveChangesAsync();

        // --- Create assignments (one per course) ---
        var assignments = new[]
        {
            new Assignment
            {
                Title = "Hello World Program",
                Description = "Write a program that prints 'Hello, World!' in a language of your choice. Include comments explaining each line.",
                DueDate = DateTime.UtcNow.AddDays(14),
                CourseId = courses[0].Id
            },
            new Assignment
            {
                Title = "Build a Personal Portfolio Page",
                Description = "Create a responsive portfolio page using HTML, CSS, and Razor views. Must include an about section, project showcase, and contact form.",
                DueDate = DateTime.UtcNow.AddDays(21),
                CourseId = courses[1].Id
            },
            new Assignment
            {
                Title = "Design an ER Diagram",
                Description = "Design an Entity-Relationship diagram for a library management system. Include at least 5 entities with proper relationships and cardinality.",
                DueDate = DateTime.UtcNow.AddDays(10),
                CourseId = courses[2].Id
            }
        };

        db.Assignments.AddRange(assignments);
        await db.SaveChangesAsync();

        // --- Create a submission for the first assignment ---
        var submission = new Submission
        {
            TextContent = "print('Hello, World!')  # This prints a greeting to the console using Python's built-in print function.",
            StudentId = student.Id,
            AssignmentId = assignments[0].Id
        };

        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        // --- Grade that submission ---
        var grade = new Grade
        {
            Score = 95.00m,
            Feedback = "Excellent work! Clean code with good comments. Consider adding error handling in future assignments.",
            SubmissionId = submission.Id,
            GradedById = instructor.Id
        };

        db.Grades.Add(grade);
        await db.SaveChangesAsync();
    }
}
