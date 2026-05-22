using LMS.DTOs;

namespace LMS.Services;

// Contract for grading submissions — only course instructors or admins can grade
public interface IGradeService
{
    // Assign a grade to an ungraded submission
    Task<GradeDto> GradeSubmissionAsync(int submissionId, string userId, bool isAdmin, GradeSubmissionRequest request);
    // Update an existing grade on a previously graded submission
    Task<GradeDto> UpdateGradeAsync(int submissionId, string userId, bool isAdmin, GradeSubmissionRequest request);
    // Get all submissions (with grades) for a student in a specific course
    Task<IList<SubmissionDto>> GetGradesForStudentCourseAsync(string studentId, int courseId);
}
