using LMS.DTOs;

namespace LMS.Services;

public interface ICourseService
{
    Task<CourseDto> CreateAsync(string instructorId, CreateCourseRequest request);
    Task<IList<CourseDto>> GetAllAsync();
    Task<CourseDetailDto> GetByIdAsync(int id);
    Task<CourseDto> UpdateAsync(int id, string instructorId, UpdateCourseRequest request);
    Task DeleteAsync(int id, string instructorId);
    Task EnrollStudentAsync(int courseId, string studentId);
    Task UnenrollStudentAsync(int courseId, string studentId);
    Task<IList<CourseDto>> GetByInstructorAsync(string instructorId);
    Task<IList<CourseDto>> GetEnrolledCoursesAsync(string studentId);
}
