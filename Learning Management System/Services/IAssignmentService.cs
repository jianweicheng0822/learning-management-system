using LMS.DTOs;

namespace LMS.Services;

public interface IAssignmentService
{
    Task<AssignmentDto> CreateAsync(int courseId, string userId, bool isAdmin, CreateAssignmentRequest request);
    Task<AssignmentDto> GetByIdAsync(int id);
    Task<IList<AssignmentDto>> GetByCourseAsync(int courseId);
    Task<AssignmentDto> UpdateAsync(int id, string userId, bool isAdmin, UpdateAssignmentRequest request);
    Task DeleteAsync(int id, string userId, bool isAdmin);
}
