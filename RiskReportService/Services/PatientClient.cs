using RiskReportService.Models;
using RiskReportService.Services.Interfaces;

namespace RiskReportService.Services
{
    public class PatientClient : IPatientClient
    {
        private readonly HttpClient _httpClient;

        public PatientClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PatientDto?> GetPatientByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/patients/{id}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur appel Patient API : {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PatientDto>();
        }
    }
}
