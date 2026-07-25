using FluentAssertions;
using MWMS.Application.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using System.Text.Json;

namespace MWMS.IntegrationTests.Controllers;

public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_ForInvalidCredentials()
    {
        var request = new LoginRequest { Username = "invalid", Password = "wrongpassword" };
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_ForValidAdmin()
    {
        var request = new LoginRequest { Username = "admin", Password = "Password123!" };
        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);
        
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }
}
