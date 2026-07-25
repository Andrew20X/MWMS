using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MWMS.API.Controllers;
using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MWMS.UnitTests.Controllers;

public class DashboardControllerTests
{
    private readonly Mock<IDashboardService> _dashboardServiceMock;
    private readonly DashboardController _sut;

    public DashboardControllerTests()
    {
        _dashboardServiceMock = new Mock<IDashboardService>();
        _sut = new DashboardController(_dashboardServiceMock.Object);
    }

    [Fact]
    public async Task GetTodayStats_ShouldReturnOkResult_WithStats()
    {
        // Arrange
        var stats = new DashboardStatsDto { TotalEmployees = 10, PresentToday = 5, LateArrivals = 2, Absent = 5 };
        _dashboardServiceMock.Setup(x => x.GetTodayStatsAsync()).ReturnsAsync(stats);

        // Act
        var result = await _sut.GetTodayStats();

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(stats);
    }

    [Fact]
    public async Task GetAttendanceTrend_ShouldReturnOkResult_WithTrend()
    {
        // Arrange
        var trend = new List<AttendanceTrendDto>
        {
            new AttendanceTrendDto { Date = "2023-01-01", PresentCount = 10, AbsentCount = 2 }
        };
        _dashboardServiceMock.Setup(x => x.GetAttendanceTrendAsync(7)).ReturnsAsync(trend);

        // Act
        var result = await _sut.GetAttendanceTrend(7);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(trend);
    }
}
