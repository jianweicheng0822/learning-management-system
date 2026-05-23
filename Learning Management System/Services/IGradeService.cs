using LMS.DTOs;

namespace LMS.Services;

public interface IGradeService
{
    Task<ServiceResult<GradeDto>> GradeSubmissionAsync(int submissionId, string userId, bool isAdmin, GradeSubmissionRequest request);
    Task<ServiceResult<GradeDto>> UpdateGradeAsync(int submissionId, string userId, bool isAdmin, GradeSubmissionRequest request);
    Task<ServiceResult<PagedResult<SubmissionDto>>> GetGradesForStudentCourseAsync(string studentId, int courseId, int page = 1, int pageSize = 10);
    Task<decimal?> GetAverageScoreAsync(string studentId, int courseId);
}
