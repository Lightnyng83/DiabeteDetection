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
using Commons.Security.Service;

namespace NotesServiceTests
{
    public class NotesControllerTests : IClassFixture<CustomWebApplicationFactory>, IClassFixture<TokenServiceFixture>
    {
        private readonly HttpClient _client;
        private readonly ITokenService _tokenService;

        public NotesControllerTests(CustomWebApplicationFactory factory, TokenServiceFixture fixture)
        {
            _client = factory.CreateClient();
            _client.BaseAddress = new Uri("https://localhost:7041/");
            _tokenService = fixture.TokenService;
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
            _tokenService.AuthenticateClient(_client,60);
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
            _tokenService.AuthenticateClient(_client, 60);
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
            _tokenService.AuthenticateClient(_client, 60);
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
            _tokenService.AuthenticateClient(_client, 60);
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
            _tokenService.AuthenticateClient(_client, 60);
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
            _tokenService.AuthenticateClient(_client, 60);
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
            _tokenService.AuthenticateClient(_client, 60);
            var note = new Note
            {
                PatientId = Guid.NewGuid(),
                Content = "Note originale"
            };

            // Créer la note
            var createResponse = await _client.PostAsJsonAsync("/api/notes", note);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<Note>();
            Assert.NotNull(created);

            // Sauvegarder la date de création initiale
            var originalCreatedAt = created!.CreatedAt;

            // Modifier le contenu de la note
            created.Content = "Contenu mis à jour";

            // Act : Mettre à jour la note
            var updateResponse = await _client.PutAsJsonAsync($"/api/notes/{created.Id}", created);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            // Vérifier via une récupération
            var getResponse = await _client.GetAsync($"/api/notes/{created.Id}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var updatedNote = await getResponse.Content.ReadFromJsonAsync<Note>();
            Assert.NotNull(updatedNote);
            Assert.Equal("Contenu mis à jour", updatedNote!.Content);

            // Vérifier que la différence en millisecondes est inférieure à 1 ms
            var diffMs = Math.Abs((updatedNote.CreatedAt - originalCreatedAt).TotalMilliseconds);
            Assert.True(diffMs < 1, $"La différence entre la date originale et la date mise à jour est trop grande ({diffMs} ms)");
        }


        [Fact]
        public async Task DeleteNote_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            _tokenService.AuthenticateClient(_client, 60);
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
            _tokenService.AuthenticateClient(_client, 60);

            // Act
            var response = await _client.DeleteAsync("/api/notes/1");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
