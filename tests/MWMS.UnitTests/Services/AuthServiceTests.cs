using FluentAssertions;
using Moq;
using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MWMS.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _emailServiceMock = new Mock<IEmailService>();

        _sut = new AuthService(
            _userRepositoryMock.Object,
            _employeeRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _emailServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User)null);

        var request = new LoginRequest { Username = "test", Password = "password" };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var user = new User { Username = "test", PasswordHash = "hash" };
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test")).ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.Verify("wrong_password", "hash")).Returns(false);

        var request = new LoginRequest { Username = "test", Password = "wrong_password" };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User { Id = 1, Username = "test", PasswordHash = "hash", Role = "Admin" };
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test")).ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.Verify("correct_password", "hash")).Returns(true);
        _jwtTokenGeneratorMock.Setup(x => x.GenerateToken(user.Id, user.Username, "Admin", null)).Returns("mock_token");

        var request = new LoginRequest { Username = "test", Password = "correct_password" };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("mock_token");
        result.Username.Should().Be("test");
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task ForgotPasswordAsync_ShouldThrowException_WhenUserOrEmailIsInvalid()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync("test")).ReturnsAsync((User)null);
        var request = new ForgotPasswordRequest { Username = "test", Email = "test@test.com" };

        // Act & Assert
        await _sut.Invoking(x => x.ForgotPasswordAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid username or email address.");
    }
}
