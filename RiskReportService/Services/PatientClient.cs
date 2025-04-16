using Microsoft.IdentityModel.Tokens;
using RiskReportService.Models;
using RiskReportService.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace RiskReportService.Services
{
    public class PatientClient : IPatientClient
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _service;

        public PatientClient(HttpClient httpClient, IServiceProvider service)
        {
            _httpClient = httpClient;
            _service = service;
        }

        public async Task<PatientDto?> GetPatientByIdAsync(Guid id)
        {
            AuthenticateClient(_service);
            var response = await _httpClient.GetAsync($"api/patients/{id}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur appel Patient API : {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<PatientDto>();
        }
        private string GenerateTestJwtToken(IConfiguration configuration)
        {
            var secretKey = configuration["JwtSettings:SecretKey"];
            var issuer = configuration["JwtSettings:Issuer"];
            var audience = configuration["JwtSettings:Audience"];
            var expiryMinutes = 60;

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                    new Claim(ClaimTypes.Name, "testuser")
                }),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private void AuthenticateClient(IServiceProvider services)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var token = GenerateTestJwtToken(configuration);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
