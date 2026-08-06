using FluentAssertions;
using Moq;
using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Domain.Enums;
using MWMS.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MWMS.UnitTests.Services;

public class AttendanceServiceTests
{
    private readonly Mock<IAttendanceRepository> _attendanceRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IOvertimeRepository> _overtimeRepositoryMock;
    private readonly Mock<IGenericRepository<RawAttendanceLog>> _rawLogRepositoryMock;
    private readonly Mock<ISalaryDeductionRepository> _deductionRepositoryMock;
    private readonly Mock<IGenericRepository<LeaveRequest>> _leaveRequestRepositoryMock;
    private readonly AttendanceService _sut;

    public AttendanceServiceTests()
    {
        _attendanceRepositoryMock = new Mock<IAttendanceRepository>();
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _overtimeRepositoryMock = new Mock<IOvertimeRepository>();
        _rawLogRepositoryMock = new Mock<IGenericRepository<RawAttendanceLog>>();
        _deductionRepositoryMock = new Mock<ISalaryDeductionRepository>();
        _leaveRequestRepositoryMock = new Mock<IGenericRepository<LeaveRequest>>();

        _sut = new AttendanceService(
            _attendanceRepositoryMock.Object,
            _employeeRepositoryMock.Object,
            _overtimeRepositoryMock.Object,
            _rawLogRepositoryMock.Object,
            _deductionRepositoryMock.Object,
            _leaveRequestRepositoryMock.Object);
    }

    [Fact]
    public async Task CheckInAsync_ShouldThrowException_WhenEmployeeNotFound()
    {
        // Arrange
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Employee)null);

        // Act & Assert
        await _sut.Invoking(x => x.CheckInAsync(1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee not found.");
    }

    [Fact]
    public async Task CheckInAsync_ShouldThrowException_WhenAlreadyCheckedIn()
    {
        // Arrange
        var employee = new Employee { Id = 1 };
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        _attendanceRepositoryMock.Setup(x => x.GetByEmployeeAndDateAsync(1, It.IsAny<DateOnly>())).ReturnsAsync(new Attendance());

        // Act & Assert
        await _sut.Invoking(x => x.CheckInAsync(1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee has already checked in today.");
    }

    [Fact]
    public async Task CheckInAsync_ShouldSetStatusPresent_WhenOnTime()
    {
        // Arrange
        var shift = new Shift { StartTime = TimeOnly.FromTimeSpan(DateTime.Now.AddHours(1).TimeOfDay), GraceMinutes = 15 };
        var employee = new Employee { Id = 1, Shift = shift };
        
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        _attendanceRepositoryMock.Setup(x => x.GetByEmployeeAndDateAsync(1, It.IsAny<DateOnly>())).ReturnsAsync((Attendance)null);

        // Act
        var result = await _sut.CheckInAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.IsLate.Should().BeFalse();
        result.LateMinutes.Should().Be(0);
        _attendanceRepositoryMock.Verify(x => x.AddAsync(It.Is<Attendance>(a => a.Status == AttendanceStatus.Present)), Times.Once);
        _attendanceRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
