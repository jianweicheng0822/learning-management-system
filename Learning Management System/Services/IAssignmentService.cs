using LMS.DTOs;

namespace LMS.Services;

public interface IAssignmentService
{
    Task<ServiceResult<AssignmentDto>> CreateAsync(int courseId, string userId, bool isAdmin, CreateAssignmentRequest request);
    Task<ServiceResult<AssignmentDto>> GetByIdAsync(int id);
    Task<ServiceResult<PagedResult<AssignmentDto>>> GetByCourseAsync(int courseId, int page = 1, int pageSize = 10);
    Task<ServiceResult<AssignmentDto>> UpdateAsync(int id, string userId, bool isAdmin, UpdateAssignmentRequest request);
    Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin);
}
