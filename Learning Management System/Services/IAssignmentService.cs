using LMS.DTOs;

namespace LMS.Services;

// Contract for assignment CRUD — all mutations require course-owner or admin access
public interface IAssignmentService
{
    // Creates an assignment under a course; only the course instructor or admin can call this
    Task<AssignmentDto> CreateAsync(int courseId, string userId, bool isAdmin, CreateAssignmentRequest request);
    Task<AssignmentDto> GetByIdAsync(int id);
    // Returns all assignments for a given course
    Task<IList<AssignmentDto>> GetByCourseAsync(int courseId);
    Task<AssignmentDto> UpdateAsync(int id, string userId, bool isAdmin, UpdateAssignmentRequest request);
    Task DeleteAsync(int id, string userId, bool isAdmin);
}
