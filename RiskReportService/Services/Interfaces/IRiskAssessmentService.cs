using RiskReportService.Models;

namespace RiskReportService.Services.Interfaces
{
    public interface IRiskAssessmentService
    {
        Task<RiskLevel> AssessRiskAsync(Guid patientId);
    }
}
