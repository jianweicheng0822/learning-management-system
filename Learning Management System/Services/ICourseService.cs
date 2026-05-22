using LMS.DTOs;

namespace LMS.Services;

// Contract for course CRUD, enrollment management, and course listing queries
public interface ICourseService
{
    Task<CourseDto> CreateAsync(string instructorId, CreateCourseRequest request);
    Task<IList<CourseDto>> GetAllAsync();
    // Returns full course details including enrolled students and assignments
    Task<CourseDetailDto> GetByIdAsync(int id);
    // Only the course owner or an admin can update
    Task<CourseDto> UpdateAsync(int id, string userId, bool isAdmin, UpdateCourseRequest request);
    // Only the course owner or an admin can delete
    Task DeleteAsync(int id, string userId, bool isAdmin);
    // Enroll a student; throws if already enrolled
    Task EnrollStudentAsync(int courseId, string studentId);
    Task UnenrollStudentAsync(int courseId, string studentId);
    // Courses where this user is the instructor
    Task<IList<CourseDto>> GetByInstructorAsync(string instructorId);
    // Courses where this student is enrolled
    Task<IList<CourseDto>> GetEnrolledCoursesAsync(string studentId);
}
