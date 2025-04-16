using RiskReportService.Models;
using RiskReportService.Services.Interfaces;
using System.Text.RegularExpressions;

namespace RiskReportService.Services
{
    public class RiskAssessmentService : IRiskAssessmentService
    {
        private readonly IPatientClient _patientClient;
        private readonly INoteClient _noteClient;

        private readonly List<string> _triggers = new()
        {
            "Hémoglobine A1C", "Microalbumine", "Taille", "Poids", "Fumeur",
            "Fumeuse", "Anormal", "Cholestérol", "Vertiges", "Rechute",
            "Réaction", "Anticorps"
        };

        public RiskAssessmentService(IPatientClient patientClient, INoteClient noteClient)
        {
            _patientClient = patientClient;
            _noteClient = noteClient;
        }

        public async Task<RiskLevel> AssessRiskAsync(Guid patientId)
        {
            var patient = await _patientClient.GetPatientByIdAsync(patientId);
            var notes = await _noteClient.GetNotesByPatientIdAsync(patientId);

            if (patient == null) return RiskLevel.None;

            var age = DateTime.Now.Year - patient.DateDeNaissance.Year;
            if (patient.DateDeNaissance.Date > DateTime.Now.AddYears(-age)) age--;

            var triggerCount = notes
                .Select(n => n.Content)
                .Where(c => !string.IsNullOrEmpty(c))
                .SelectMany(c => _triggers.Where(t => c.Contains(t, StringComparison.OrdinalIgnoreCase)))
                .Count();

            // Évaluation du risque
            if (triggerCount == 0)
                return RiskLevel.None;

            if (age > 30)
            {
                if (triggerCount >= 8)
                    return RiskLevel.EarlyOnset;
                if (triggerCount >= 6)
                    return RiskLevel.InDanger;
                if (triggerCount >= 2)
                    return RiskLevel.Borderline;
            }
            else
            {
                if (patient.Genre == 0)
                {
                    if (triggerCount >= 5)
                        return RiskLevel.EarlyOnset;
                    if (triggerCount >= 3)
                        return RiskLevel.InDanger;
                }
                else
                {
                    if (triggerCount >= 7)
                        return RiskLevel.EarlyOnset;
                    if (triggerCount >= 4)
                        return RiskLevel.InDanger;
                }
            }

            return RiskLevel.None;
        }
    }
}