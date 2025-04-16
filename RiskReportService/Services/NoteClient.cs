using RiskReportService.Models;
using RiskReportService.Services.Interfaces;

namespace RiskReportService.Services
{
    public class NoteClient : INoteClient
    {
        private readonly HttpClient _httpClient;

        public NoteClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<NoteDto>> GetNotesByPatientIdAsync(Guid patientId)
        {
            var response = await _httpClient.GetAsync($"api/notes/patient/{patientId}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur appel Notes API : {response.StatusCode}");
                return new List<NoteDto>();
            }

            return await response.Content.ReadFromJsonAsync<List<NoteDto>>() ?? new List<NoteDto>();
        }
    }
}
