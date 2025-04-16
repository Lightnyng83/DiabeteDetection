using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PatientService;
using PatientService.Models;
using Xunit;

namespace PatientServiceTests
{
    public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly IServiceProvider _serviceProvider;

        public AccountControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _serviceProvider = factory.Services;
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginRequest = new
            {
                Username = "wronguser",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/account/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_ReturnsOkAndToken_WhenCredentialsAreValid()
        {
            // Arrange
            var username = "testuser";
            var password = "Test@12345";

            // Crée l'utilisateur dans la base de test
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                UserName = username,
                Email = "test@example.com"
            };

            var result = await userManager.CreateAsync(user, password);
            Assert.True(result.Succeeded, $"Échec création utilisateur : {string.Join(", ", result.Errors.Select(e => e.Description))}");

            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/account/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(content);
            Assert.False(string.IsNullOrEmpty(content!.Token));
        }


        private class LoginResponse
        {
            public string? Token { get; set; }
            public string? Message { get; set; }
        }

        public class LoginRequest
        {
            public required string Username { get; set; }
            public required string Password { get; set; }
        }
    }
}