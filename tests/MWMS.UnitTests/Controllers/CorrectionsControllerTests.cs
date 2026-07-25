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

public class CorrectionsControllerTests
{
    private readonly Mock<ICorrectionRepository> _correctionRepoMock;
    private readonly Mock<IAttendanceRepository> _attendanceRepoMock;
    private readonly Mock<IEmployeeRepository> _employeeRepoMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly CorrectionsController _sut;

    public CorrectionsControllerTests()
    {
        _correctionRepoMock = new Mock<ICorrectionRepository>();
        _attendanceRepoMock = new Mock<IAttendanceRepository>();
        _employeeRepoMock = new Mock<IEmployeeRepository>();
        _emailServiceMock = new Mock<IEmailService>();

        _sut = new CorrectionsController(
            _correctionRepoMock.Object,
            _attendanceRepoMock.Object,
            _employeeRepoMock.Object,
            _emailServiceMock.Object);

        // Setup User context
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("FullName", "Admin User")
        }, "mock"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task ApproveCorrection_AppendsFormattedNote()
    {
        // Arrange
        var request = new CorrectionRequest
        {
            Id = 1,
            EmployeeId = 1,
            Status = "Pending",
            Reason = "Forgot to sign out"
        };
        _correctionRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(request);
        _employeeRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(new Employee { Id = 1, Email = "test@example.com" });

        // Act
        var result = await _sut.ApproveCorrection(1, new CorrectionsController.CorrectionActionDto { Note = "Approved correction" });

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var updatedRequest = okResult.Value.Should().BeOfType<CorrectionRequest>().Subject;

        updatedRequest.Status.Should().Be("Approved");
        updatedRequest.Reason.Should().Contain("* Approval Status: Approved");
        updatedRequest.Reason.Should().Contain("* Approved By: Admin User");
        updatedRequest.Reason.Should().Contain("* Admin Note: Approved correction");
    }
}
