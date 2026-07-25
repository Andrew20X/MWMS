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

public class LeaveServiceTests
{
    private readonly Mock<IGenericRepository<LeaveRequest>> _leaveRepositoryMock;
    private readonly Mock<IEmployeeRepository> _employeeRepositoryMock;
    private readonly Mock<IGenericRepository<User>> _userRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILeaveBalanceRepository> _leaveBalanceRepositoryMock;
    private readonly Mock<IGenericRepository<ApprovalHistory>> _approvalHistoryRepositoryMock;
    private readonly Mock<IGenericRepository<Attendance>> _attendanceRepositoryMock;
    private readonly LeaveService _sut;

    public LeaveServiceTests()
    {
        _leaveRepositoryMock = new Mock<IGenericRepository<LeaveRequest>>();
        _employeeRepositoryMock = new Mock<IEmployeeRepository>();
        _userRepositoryMock = new Mock<IGenericRepository<User>>();
        _emailServiceMock = new Mock<IEmailService>();
        _leaveBalanceRepositoryMock = new Mock<ILeaveBalanceRepository>();
        _approvalHistoryRepositoryMock = new Mock<IGenericRepository<ApprovalHistory>>();
        _attendanceRepositoryMock = new Mock<IGenericRepository<Attendance>>();

        _sut = new LeaveService(
            _leaveRepositoryMock.Object,
            _employeeRepositoryMock.Object,
            _userRepositoryMock.Object,
            _emailServiceMock.Object,
            _leaveBalanceRepositoryMock.Object,
            _approvalHistoryRepositoryMock.Object,
            _attendanceRepositoryMock.Object);
    }

    [Fact]
    public async Task SubmitRequestAsync_ShouldThrowException_WhenEmployeeNotFound()
    {
        // Arrange
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Employee)null);
        var request = new CreateLeaveRequestDto { EmployeeId = 1 };

        // Act & Assert
        await _sut.Invoking(x => x.SubmitRequestAsync(request))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Employee not found.");
    }

    [Fact]
    public async Task SubmitRequestAsync_ShouldSetPendingHRApproval_WhenEmployeeHasNoManager()
    {
        // Arrange
        var employee = new Employee { Id = 1, ManagerId = null };
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        var request = new CreateLeaveRequestDto { EmployeeId = 1, Type = LeaveType.Annual, StartDate = DateOnly.FromDateTime(DateTime.Today), EndDate = DateOnly.FromDateTime(DateTime.Today) };

        // Act
        var result = await _sut.SubmitRequestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(LeaveStatus.PendingHRApproval.ToString());
        _leaveRepositoryMock.Verify(x => x.AddAsync(It.Is<LeaveRequest>(lr => lr.Status == LeaveStatus.PendingHRApproval)), Times.Once);
        _leaveRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitRequestAsync_ShouldSetPendingManagerApproval_WhenEmployeeHasManager()
    {
        // Arrange
        var employee = new Employee { Id = 1, ManagerId = 2 };
        _employeeRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        var request = new CreateLeaveRequestDto { EmployeeId = 1, Type = LeaveType.Sick, StartDate = DateOnly.FromDateTime(DateTime.Today), EndDate = DateOnly.FromDateTime(DateTime.Today) };

        // Act
        var result = await _sut.SubmitRequestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(LeaveStatus.PendingManagerApproval.ToString());
        _leaveRepositoryMock.Verify(x => x.AddAsync(It.Is<LeaveRequest>(lr => lr.Status == LeaveStatus.PendingManagerApproval)), Times.Once);
        _leaveRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    [Fact]
    public async Task ApproveRequestAsync_Manager_AppendsFormattedNote()
    {
        // Arrange
        var request = new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            Status = LeaveStatus.PendingManagerApproval,
            Reason = "Initial Reason"
        };
        _leaveRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(request);

        // Act
        var result = await _sut.ApproveRequestAsync(1, 2, "Manager Bob", "Manager", "Looks good");

        // Assert
        result.Should().BeTrue();
        request.Reason.Should().Contain("* Approval Status: Approved by Manager");
        request.Reason.Should().Contain("* Approved By: Manager Bob");
        request.Reason.Should().Contain("* Admin Note: Looks good");
        request.Status.Should().Be(LeaveStatus.PendingHRApproval);
    }
}
