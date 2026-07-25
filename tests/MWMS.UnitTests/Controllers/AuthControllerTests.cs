using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MWMS.API.Controllers;
using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MWMS.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _sut = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenResultIsNull()
    {
        // Arrange
        var request = new LoginRequest { Username = "test", Password = "password" };
        _authServiceMock.Setup(x => x.LoginAsync(request)).ReturnsAsync((LoginResponse)null);

        // Act
        var result = await _sut.Login(request);

        // Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_WhenResultIsNotNull()
    {
        // Arrange
        var request = new LoginRequest { Username = "test", Password = "password" };
        var response = new LoginResponse { Token = "token", Username = "test", Role = "Admin" };
        _authServiceMock.Setup(x => x.LoginAsync(request)).ReturnsAsync(response);

        // Act
        var result = await _sut.Login(request);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(response);
    }
}
