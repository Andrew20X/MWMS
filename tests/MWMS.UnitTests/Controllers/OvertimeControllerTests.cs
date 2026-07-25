using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MWMS.API.Controllers;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MWMS.UnitTests.Controllers;

public class OvertimeControllerTests
{
    private readonly Mock<IOvertimeRepository> _overtimeRepoMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IGenericRepository<ApprovalHistory>> _approvalRepoMock;
    private readonly OvertimeController _sut;

    public OvertimeControllerTests()
    {
        _overtimeRepoMock = new Mock<IOvertimeRepository>();
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _approvalRepoMock = new Mock<IGenericRepository<ApprovalHistory>>();

        _sut = new OvertimeController(
            _overtimeRepoMock.Object,
            _employeeRepoMock.Object,
            _emailServiceMock.Object,
            _approvalRepoMock.Object);

        // Setup User context
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Role, "Manager"),
            new Claim("FullName", "Manager Bob"),
            new Claim(ClaimTypes.NameIdentifier, "2")
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task ApproveOvertime_ByManager_AppendsFormattedNote()
    {
        // Arrange
        var request = new OvertimeRequest
        {
            Id = 1,
            EmployeeId = 1,
            Status = OvertimeRequest.StatusPendingManager,
            Reason = "Extra hours"
        };
        _overtimeRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(request);

        // Act
        var result = await _sut.ApproveOvertime(1, "Looks good to me");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var updatedRequest = okResult.Value.Should().BeOfType<OvertimeRequest>().Subject;

        updatedRequest.Status.Should().Be(OvertimeRequest.StatusPendingHR);
        updatedRequest.Reason.Should().Contain("* Approval Status: Approved by Manager");
        updatedRequest.Reason.Should().Contain("* Approved By: Manager Bob");
        updatedRequest.Reason.Should().Contain("* Admin Note: Looks good to me");
    }
}
