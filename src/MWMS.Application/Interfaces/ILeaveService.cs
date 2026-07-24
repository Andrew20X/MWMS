using MWMS.Application.DTOs;
using MWMS.Domain.Entities;

namespace MWMS.Application.Interfaces;

public interface ILeaveService
{
    Task<LeaveRequestDto> SubmitRequestAsync(CreateLeaveRequestDto dto);

    Task<IEnumerable<LeaveRequestDto>> GetEmployeeLeavesAsync(int employeeId);

    Task<IEnumerable<LeaveRequestDto>> GetPendingLeavesAsync();

    Task<IEnumerable<LeaveRequestDto>> GetManagerPendingLeavesAsync(int managerId);

    Task<IEnumerable<LeaveRequestDto>> GetHRPendingLeavesAsync();

    Task<IEnumerable<LeaveRequestDto>> GetAllLeavesAsync();

    /// <summary>
    /// Advances the leave request through the approval workflow.
    /// - Manager role: moves from PendingManagerApproval → PendingHRApproval.
    /// - Admin (HR) role: moves from PendingHRApproval → Approved (and deducts balance).
    /// </summary>
    Task<bool> ApproveRequestAsync(int requestId, int approverId, string approverName, string callerRole, string? adminMessage = null);

    /// <summary>Rejects the request at any stage. Both Manager and Admin can reject.</summary>
    Task<bool> RejectRequestAsync(int requestId, int approverId, string approverName, string callerRole, string? adminMessage = null);

    Task<bool> DeleteRequestAsync(int requestId, int employeeId, bool isAdmin);

    Task<bool> DeleteAllEmployeeRequestsAsync(int employeeId);

    Task<bool> DeleteAllRequestsAsync();

    /// <summary>Returns the leave balance for an employee for the given year (auto-creates if missing).</summary>
    Task<LeaveBalanceDto> GetLeaveBalanceAsync(int employeeId, int year);

    /// <summary>Updates the leave balance totals for an employee.</summary>
    Task<LeaveBalanceDto> UpdateLeaveBalanceAsync(int employeeId, int year, UpdateLeaveBalanceDto dto);

    /// <summary>Returns all approval/rejection history entries for a specific leave request.</summary>
    Task<IEnumerable<ApprovalHistoryDto>> GetApprovalHistoryAsync(int requestId, string requestType);
}

