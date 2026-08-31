using SchoolOperations.DTOs.AcademicPerformance;

namespace SchoolOperations.Services
{
    public interface IAcademicPerformanceService
    {
        Task<List<PoorPerformanceSummaryDto>>
            GetStudentsBelowPerformanceThresholdAsync(
                decimal threshold,
                string? subject);

        Task<List<DecliningPerformanceSummaryDto>>
            GetStudentsWithDecliningPerformanceAsync(
                string? subject);
    }
}