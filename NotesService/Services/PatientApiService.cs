using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotesService.Services
{
    public class PatientApiService
    {
        private readonly HttpClient _httpClient;

        public PatientApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PatientDto>> GetAllPatientsAsync()
        {
            var patients = await _httpClient.GetFromJsonAsync<List<PatientDto>>("/api/patients");
            return patients ?? new List<PatientDto>();
        }
    }

    public class PatientDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
    }
}