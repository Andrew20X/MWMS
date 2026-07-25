using FluentAssertions;
using MWMS.Application.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace MWMS.IntegrationTests.Controllers;

public class EmployeesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EmployeesControllerIntegrationTests(CustomWebApplicationFactory<Program> factory)
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

    [Fact]
    public async Task GetAll_ShouldReturnList_WhenAuthenticated()
    {
        await AuthenticateAsync();
        var response = await _client.GetAsync("/api/Employees");
        response.EnsureSuccessStatusCode();
    }
}
