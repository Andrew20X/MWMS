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

public class DashboardServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IAttendanceRepository> _attendanceRepositoryMock;
    private readonly DashboardService _sut;

    public DashboardServiceTests()
    {
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _attendanceRepositoryMock = new Mock<IAttendanceRepository>();

        _sut = new DashboardService(_employeeRepositoryMock.Object, _attendanceRepositoryMock.Object);
    }

    [Fact]
    public async Task GetTodayStatsAsync_ShouldReturnCorrectStats()
    {
        // Arrange
        var employees = new List<Employee>
        {
            new Employee { Id = 1, IsActive = true },
            new Employee { Id = 2, IsActive = true },
            new Employee { Id = 3, IsActive = false }
        };

        var shift = new Shift { StartTime = new TimeOnly(9, 0, 0) };

        var attendances = new List<Attendance>
        {
            new Attendance { EmployeeId = 1, CheckIn = new TimeOnly(8, 50, 0), Employee = new Employee { Shift = shift } },
            new Attendance { EmployeeId = 2, CheckIn = new TimeOnly(9, 15, 0), Employee = new Employee { Shift = shift } } // Late
        };

        _employeeRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(employees);
        _attendanceRepositoryMock.Setup(x => x.GetTodayAttendanceAsync(It.IsAny<DateOnly>())).ReturnsAsync(attendances);

        // Act
        var result = await _sut.GetTodayStatsAsync();

        // Assert
        result.TotalEmployees.Should().Be(2); // 2 active
        result.PresentToday.Should().Be(2);
        result.LateArrivals.Should().Be(1);
        result.Absent.Should().Be(0);
    }
}
