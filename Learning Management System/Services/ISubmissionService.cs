using LMS.DTOs;

namespace LMS.Services;

// Contract for student assignment submissions
public interface ISubmissionService
{
    // Submit work for an assignment; enforces enrollment and one-submission-per-student
    Task<SubmissionDto> SubmitAsync(int assignmentId, string studentId, CreateSubmissionRequest request);
    Task<SubmissionDto> GetByIdAsync(int id);
    // All submissions for a given assignment (used by instructors for grading)
    Task<IList<SubmissionDto>> GetByAssignmentAsync(int assignmentId);
    // All submissions by a specific student across assignments
    Task<IList<SubmissionDto>> GetByStudentAsync(string studentId);
}
