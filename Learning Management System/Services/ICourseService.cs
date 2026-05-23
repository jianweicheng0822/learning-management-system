using LMS.DTOs;

namespace LMS.Services;

public interface ICourseService
{
    Task<ServiceResult<CourseDto>> CreateAsync(string instructorId, CreateCourseRequest request);
    Task<ServiceResult<PagedResult<CourseDto>>> GetAllAsync(int page = 1, int pageSize = 9);
    Task<ServiceResult<CourseDetailDto>> GetByIdAsync(int id);
    Task<ServiceResult<CourseDto>> UpdateAsync(int id, string userId, bool isAdmin, UpdateCourseRequest request);
    Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin);
    Task<ServiceResult> EnrollStudentAsync(int courseId, string studentId);
    Task<ServiceResult> UnenrollStudentAsync(int courseId, string studentId);
    Task<ServiceResult<PagedResult<CourseDto>>> GetByInstructorAsync(string instructorId, int page = 1, int pageSize = 9);
    Task<ServiceResult<PagedResult<CourseDto>>> GetEnrolledCoursesAsync(string studentId, int page = 1, int pageSize = 9);
}
