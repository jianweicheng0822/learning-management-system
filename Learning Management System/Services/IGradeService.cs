using LMS.DTOs;

namespace LMS.Services;

public interface IGradeService
{
    Task<GradeDto> GradeSubmissionAsync(int submissionId, string userId, bool isAdmin, GradeSubmissionRequest request);
    Task<GradeDto> UpdateGradeAsync(int submissionId, string userId, bool isAdmin, GradeSubmissionRequest request);
    Task<IList<SubmissionDto>> GetGradesForStudentCourseAsync(string studentId, int courseId);
}
