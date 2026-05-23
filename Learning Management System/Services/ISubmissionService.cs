using LMS.DTOs;

namespace LMS.Services;

public interface ISubmissionService
{
    Task<ServiceResult<SubmissionDto>> SubmitAsync(int assignmentId, string studentId, CreateSubmissionRequest request);
    Task<ServiceResult<SubmissionDto>> GetByIdAsync(int id);
    Task<ServiceResult<PagedResult<SubmissionDto>>> GetByAssignmentAsync(int assignmentId, int page = 1, int pageSize = 10);
    Task<ServiceResult<PagedResult<SubmissionDto>>> GetByStudentAsync(string studentId, int page = 1, int pageSize = 10);
}
