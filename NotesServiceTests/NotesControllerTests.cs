extern alias notesAlias;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using notesAlias::NotesService.Models;

namespace NotesServiceTests
{
    public class NotesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public NotesControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7041/");
        }

        /// <summary>
        /// Authentifie le client en injectant un token JWT dans le header.
        /// </summary>
        private void AuthenticateClient()
        {
            var token = GenerateTestJwtToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task GetNotesForPatient_Unauthorized_WhenNoTokenProvided()
        {
            // Appel GET sans token d'authentification
            var response = await _client.GetAsync($"/api/notes/patient/11111111-1111-1111-1111-111111111111");


            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetNotesForPatient_ReturnsOk_WhenAuthenticated()
        {
            // Arrange
            AuthenticateClient();
            // Utiliser un patientId arbitraire (ici aucun note n'existe forcément, mais l'endpoint doit renvoyer 200 OK).
            var patientId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/notes/patient/{Guid.NewGuid()}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateNote_ReturnsCreated_WhenValid()
        {
            // Arrange
            AuthenticateClient();
            var note = new Note
            {
                PatientId = Guid.NewGuid(),
                Content = "Test note content"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notes", note);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var created = await response.Content.ReadFromJsonAsync<Note>();
            Assert.NotNull(created);
            Assert.Equal(note.Content, created!.Content);
        }


        [Fact]
        public async Task CreateNote_ReturnsBadRequest_WhenInvalid()
        {
            // Arrange
            AuthenticateClient();
            // Note invalide (PatientId vide)
            var note = new Note
            {
                PatientId = Guid.Empty,
                Content = "Note invalide"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/notes", note);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetNote_ReturnsOk_WhenValid()
        {
            // Arrange
            AuthenticateClient();
            var note = new Note
            {
                PatientId = Guid.NewGuid(),
                Content = "Test récupération note"
            };

            // D'abord, créer la note
            var createResponse = await _client.PostAsJsonAsync("/api/notes", note);
            var created = await createResponse.Content.ReadFromJsonAsync<Note>();

            // Act
            var getResponse = await _client.GetAsync($"/api/notes/{created!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var retrieved = await getResponse.Content.ReadFromJsonAsync<Note>();
            Assert.NotNull(retrieved);
            Assert.Equal(created.Id, retrieved!.Id);
        }

        [Fact]
        public async Task GetNote_ReturnsNotFound_WhenNonExistent()
        {
            // Arrange
            AuthenticateClient();
            var nonExistentId = "nonexistentid";

            // Act
            var response = await _client.GetAsync($"/api/notes/{nonExistentId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNote_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            AuthenticateClient();
            var note = new Note
            {
                PatientId = Guid.NewGuid(),
                Content = "Note à mettre à jour"
            };

            // Créer d'abord la note
            var createResponse = await _client.PostAsJsonAsync("/api/notes", note);
            var created = await createResponse.Content.ReadFromJsonAsync<Note>();

            // Modifier la note avec un ID différent dans l'URL (mismatch)
            created!.Content = "Contenu modifié";
            var mismatchedId = "differenteId";

            // Act
            var response = await _client.PutAsJsonAsync($"/api/notes/{mismatchedId}", created);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateNote_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            AuthenticateClient();
            var note = new Note
            {
                PatientId = Guid.NewGuid(),
                Content = "Note originale"
            };

            // Créer la note
            var createResponse = await _client.PostAsJsonAsync("/api/notes", note);
            var created = await createResponse.Content.ReadFromJsonAsync<Note>();

            // Sauvegarder la date de création initiale pour vérifier sa préservation
            var originalCreatedAt = created!.CreatedAt;

            // Modifier le contenu de la note
            created.Content = "Contenu mis à jour";

            // Act
            var response = await _client.PutAsJsonAsync($"/api/notes/{created.Id}", created);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Vérifier la mise à jour via une récupération de la note
            var getResponse = await _client.GetAsync($"/api/notes/{created.Id}");
            var updatedNote = await getResponse.Content.ReadFromJsonAsync<Note>();
            Assert.NotNull(updatedNote);
            Assert.Equal("Contenu mis à jour", updatedNote!.Content);
            // <span style="color: red;">Vérification critique :</span> La date CreatedAt doit rester inchangée.
            Assert.Equal(originalCreatedAt, updatedNote.CreatedAt);
        }

        [Fact]
        public async Task DeleteNote_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            AuthenticateClient();
            var note = new Note
            {
                PatientId = Guid.NewGuid(),
                Content = "Note à supprimer"
            };

            // Créer la note
            var createResponse = await _client.PostAsJsonAsync("/api/notes", note);
            var created = await createResponse.Content.ReadFromJsonAsync<Note>();

            // Act
            var deleteResponse = await _client.DeleteAsync($"/api/notes/{created!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Vérifier que la note n'existe plus
            var getResponse = await _client.GetAsync($"/api/notes/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteNote_ReturnsNotFound_WhenNoteDoesNotExist()
        {
            // Arrange
            AuthenticateClient();

            // Act
            var response = await _client.DeleteAsync("/api/notes/nonexistentid");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

       

        [Fact]
        public async Task HealthEndpoint_ReturnsHealthy()
        {
           
            var response = await _client.GetAsync("/api/health");
            
            response.EnsureSuccessStatusCode(); // Vérifie que le status code est 200-299
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("Healthy", content);
        }


        /// <summary>
        /// Génère un token JWT pour les tests.
        /// </summary>
        /// <returns>Token JWT sous forme de chaîne</returns>
        private string GenerateTestJwtToken()
        {
            // Ces valeurs doivent correspondre à celles de la configuration de prod (appsettings.json)
            var secretKey = "MySuperSecretKey_12345MySuperSecretKey!";
            var issuer = "DiabeteDetection";
            var audience = "DiabeteDetectionUsers";
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
    }
}
