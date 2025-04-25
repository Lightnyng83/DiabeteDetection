using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Commons.Security.Service;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using PatientService;
using PatientService.Models;
using Xunit;

namespace PatientServiceTests
{
    public class PatientsControllerTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<TokenServiceFixture>
    {
        private readonly HttpClient _client;
        private readonly ITokenService _tokenService;

        public PatientsControllerTests(CustomWebApplicationFactory factory, TokenServiceFixture fixture)
        {
            _client = factory.CreateClient();
            _tokenService = fixture.TokenService;
        }

        [Fact]
        public async Task GetPatients_Unauthorized_WhenNoTokenProvided()
        {
            // Act
            var response = await _client.GetAsync("/api/patients");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetPatients_ReturnsOk_WhenAuthenticated()
        {
            // Arrange
            _tokenService.AuthenticateClient(_client,60);

            // Act
            var response = await _client.GetAsync("/api/patients");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostPatient_CreatesPatient_WhenValid()
        {
            // Arrange
            _tokenService.AuthenticateClient(_client, 60);
            var patient = new Patient
            {
                Nom = "Dupont",
                Prenom = "Jean",
                DateNaissance = new DateOnly(1980, 5, 15),
                Genre = 0, // Homme
                Adresse = "123 Rue de Paris",
                Telephone = "0600000000"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/patients", patient);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<Patient>();
            Assert.NotNull(created);
            Assert.Equal("Jean", created!.Prenom);
        }

        [Fact]
        public async Task GetPatient_ReturnsNotFound_WhenIdIsInvalid()
        {
            // Arrange
            _tokenService.AuthenticateClient(_client, 60);

            // Act
            var response = await _client.GetAsync($"/api/patients/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PutPatient_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            _tokenService.AuthenticateClient(_client, 60);
            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                Nom = "Test",
                Prenom = "Erreur",
                DateNaissance = new DateOnly(2000, 1, 1),
                Genre = 0,
                Adresse = "Test Rue",
                Telephone = "0612345678"
            };

            var url = $"/api/patients/{Guid.NewGuid()}"; // ID différent de celui dans l'objet

            // Act
            var response = await _client.PutAsJsonAsync(url, patient);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutPatient_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _tokenService.AuthenticateClient(_client, 60);
            var patient = new Patient
            {
                Nom = "Update",
                Prenom = "Test",
                DateNaissance = new DateOnly(1990, 1, 1),
                Genre = 1, // Femme
                Adresse = "456 Avenue",
                Telephone = "0611111111"
            };

            var create = await _client.PostAsJsonAsync("/api/patients", patient);
            var created = await create.Content.ReadFromJsonAsync<Patient>();

            // Modification
            created!.Nom = "Updated";
            created.Telephone = "0622222222";

            // Act
            var putResponse = await _client.PutAsJsonAsync($"/api/patients/{created.Id}", created);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
        }

        [Fact]
        public async Task DeletePatient_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _tokenService.AuthenticateClient(_client, 60);
            var patient = new Patient
            {
                Nom = "Suppression",
                Prenom = "Test",
                DateNaissance = new DateOnly(1975, 7, 7),
                Genre = 0,
                Adresse = "789 Boulevard",
                Telephone = "0699999999"
            };

            var create = await _client.PostAsJsonAsync("/api/patients", patient);
            var created = await create.Content.ReadFromJsonAsync<Patient>();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/patients/{created!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task DeletePatient_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            _tokenService.AuthenticateClient(_client, 60);

            // Act
            var response = await _client.DeleteAsync($"/api/patients/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

    }
}
