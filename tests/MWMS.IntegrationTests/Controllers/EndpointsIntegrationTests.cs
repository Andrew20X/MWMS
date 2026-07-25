using FluentAssertions;
using MWMS.Application.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MWMS.IntegrationTests.Controllers;

public class EndpointsIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EndpointsIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var loginRequest = new LoginRequest { Username = "admin", Password = "Password123!" };
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        var authResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.Token);
    }

    [Theory]
    [InlineData("/api/Employees")]
    [InlineData("/api/Leaves/all")]
    [InlineData("/api/Leaves/pending")]
    [InlineData("/api/Deductions")]
    [InlineData("/api/Overtime")]
    [InlineData("/api/Corrections")]
    [InlineData("/api/Dashboard/stats")]
    public async Task GetEndpoints_ShouldReturnOk_WhenAuthenticated(string endpoint)
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }
}
