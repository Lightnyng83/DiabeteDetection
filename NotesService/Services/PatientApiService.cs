using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Commons.Security.Service;

namespace NotesService.Services
{
    public class PatientApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenService _tokenService;


        public PatientApiService(HttpClient httpClient, ITokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }

        public async Task<List<PatientDto>> GetAllPatientsAsync(IServiceProvider services)
        {
            _tokenService.AuthenticateClient(_httpClient,60);
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