using Microsoft.IdentityModel.Tokens;
using RiskReportService.Models;
using RiskReportService.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace RiskReportService.Services
{
    public class RiskAssessmentService : IRiskAssessmentService
    {
        private readonly HttpClient _httpClient;
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

            if (patient == null)
            {
                return RiskLevel.None;
            }

            var age = DateTime.Today.Year - patient.DateNaissance.Year;
            Console.WriteLine($"[DEBUG] Patient: Age : {age}");
            if (patient.DateNaissance > DateTime.Today.AddYears(-age)) age--;
            Console.WriteLine($"[DEBUG] Patient: Date de naissance : {patient.DateNaissance}");
            Console.WriteLine($"[DEBUG] Patient: Année de naissance : {patient.DateNaissance.Year}");

            Console.WriteLine($"[DEBUG] Patient: {patient.Prenom}, Age: {age}, Genre: {patient.Genre}");
            var triggerCount = notes
                .Where(n => !string.IsNullOrWhiteSpace(n.Content))
                .Sum(note =>
                    _triggers.Sum(trigger =>
                        CountOccurrences(note.Content, trigger)
                    )
                );


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
        private int CountOccurrences(string content, string trigger)
        {
            return Regex.Matches(content, Regex.Escape(trigger), RegexOptions.IgnoreCase).Count;
        }


    }
}