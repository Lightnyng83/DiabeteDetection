using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PatientService;
using Xunit;

namespace PatientServiceTests
{
    public class AccountControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AccountControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
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
            var loginRequest = new
            {
                Username = "testuser",
                Password = "Test@12345"
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
    }
}