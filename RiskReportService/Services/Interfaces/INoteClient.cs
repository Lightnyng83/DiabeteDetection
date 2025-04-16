using RiskReportService.Models;

namespace RiskReportService.Services.Interfaces
{
    public interface INoteClient
    {
        Task<List<NoteDto>> GetNotesByPatientIdAsync(Guid patientId);
    }
}
