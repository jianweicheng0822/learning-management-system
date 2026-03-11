using LMS.DTOs;

namespace LMS.Services;

public interface ISubmissionService
{
    Task<SubmissionDto> SubmitAsync(int assignmentId, string studentId, CreateSubmissionRequest request);
    Task<SubmissionDto> GetByIdAsync(int id);
    Task<IList<SubmissionDto>> GetByAssignmentAsync(int assignmentId);
    Task<IList<SubmissionDto>> GetByStudentAsync(string studentId);
}
