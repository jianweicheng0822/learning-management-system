using LMS.DTOs;

namespace LMS.Services;

public interface ISubmissionService
{
    Task<ServiceResult<SubmissionDto>> SubmitAsync(int assignmentId, string studentId, CreateSubmissionRequest request, string? s3Key = null, string? originalFileName = null);
    Task<ServiceResult<SubmissionDto>> GetByIdAsync(int id);
    Task<ServiceResult<PagedResult<SubmissionDto>>> GetByAssignmentAsync(int assignmentId, int page = 1, int pageSize = 10);
    Task<ServiceResult<PagedResult<SubmissionDto>>> GetByStudentAsync(string studentId, int page = 1, int pageSize = 10);
    Task<ServiceResult<string>> GetDownloadUrlAsync(int submissionId, string requestingUserId, bool isInstructorOrAdmin);
    Task<SubmissionDto?> GetByStudentForAssignmentAsync(int assignmentId, string studentId);
}
