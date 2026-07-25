using FluentAssertions;
using Moq;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MWMS.UnitTests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();

        _sut = new EmployeeService(_employeeRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenValidationFails()
    {
        // Arrange
        var employee = new Employee { DepartmentId = 1, PositionId = 1, ShiftId = 1 };
        _employeeRepositoryMock.Setup(x => x.ValidateReferencesAsync(1, 1, 1)).ReturnsAsync("Invalid Department");

        // Act & Assert
        await _sut.Invoking(x => x.CreateAsync(employee))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid Department");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenEmployeeCodeIsAlreadyInUse()
    {
        // Arrange
        var employee = new Employee { DepartmentId = 1, PositionId = 1, ShiftId = 1, EmployeeCode = "123" };
        _employeeRepositoryMock.Setup(x => x.ValidateReferencesAsync(1, 1, 1)).ReturnsAsync((string)null);
        _employeeRepositoryMock.Setup(x => x.GetByEmployeeCodeAsync("123")).ReturnsAsync(new Employee());

        // Act & Assert
        await _sut.Invoking(x => x.CreateAsync(employee))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee code '123' is already in use.");
    }

    [Fact]
    public async Task CreateAsync_ShouldCallAddAsync_WhenEmployeeIsValid()
    {
        // Arrange
        var employee = new Employee { DepartmentId = 1, PositionId = 1, ShiftId = 1, EmployeeCode = "123" };
        _employeeRepositoryMock.Setup(x => x.ValidateReferencesAsync(1, 1, 1)).ReturnsAsync((string)null);
        _employeeRepositoryMock.Setup(x => x.GetByEmployeeCodeAsync("123")).ReturnsAsync((Employee)null);
        _employeeRepositoryMock.Setup(x => x.GetByDeviceUserIdAsync(It.IsAny<int>())).ReturnsAsync((Employee)null);
        _employeeRepositoryMock.Setup(x => x.AddAsync(employee)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateAsync(employee);

        // Assert
        result.Should().BeEquivalentTo(employee);
        _employeeRepositoryMock.Verify(x => x.AddAsync(employee), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenEmployeeNotFound()
    {
        // Arrange
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Employee)null);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrue_WhenEmployeeIsDeleted()
    {
        // Arrange
        var employee = new Employee { Id = 1, EmployeeCode = "123" };
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        
        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        _employeeRepositoryMock.Verify(x => x.Delete(employee), Times.Once);
    }
}
