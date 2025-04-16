using RiskReportService.Models;

namespace RiskReportService.Services.Interfaces
{
    public interface IPatientClient
    {
        Task<PatientDto?> GetPatientByIdAsync(Guid id);
    }
}
