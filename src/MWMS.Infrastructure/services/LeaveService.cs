using MWMS.Application.DTOs;
using MWMS.Application.Interfaces;
using MWMS.Domain.Entities;
using MWMS.Domain.Enums;

namespace MWMS.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly IGenericRepository<LeaveRequest> _leaveRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IGenericRepository<ApprovalHistory> _approvalHistoryRepository;

    public LeaveService(
        IGenericRepository<LeaveRequest> leaveRepository,
        IEmployeeRepository employeeRepository,
        IGenericRepository<User> userRepository,
        IEmailService emailService,
        ILeaveBalanceRepository leaveBalanceRepository,
        IGenericRepository<ApprovalHistory> approvalHistoryRepository)
    {
        _leaveRepository = leaveRepository;
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _leaveBalanceRepository = leaveBalanceRepository;
        _approvalHistoryRepository = approvalHistoryRepository;
    }

    public async Task<LeaveRequestDto> SubmitRequestAsync(CreateLeaveRequestDto dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(dto.EmployeeId);
        if (employee == null || employee.IsDeleted)
            throw new InvalidOperationException("Employee not found.");

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = dto.EmployeeId,
            Type = dto.Type,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            Status = LeaveStatus.PendingManagerApproval
        };

        await _leaveRepository.AddAsync(leaveRequest);
        await _leaveRepository.SaveChangesAsync();

        return MapToDto(leaveRequest, employee);
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetEmployeeLeavesAsync(int employeeId)
    {
        var all = await _leaveRepository.GetAllAsync();
        var employeeLeaves = all.Where(l => l.EmployeeId == employeeId && !l.IsDeleted).ToList();

        var result = new List<LeaveRequestDto>();
        foreach (var leave in employeeLeaves)
        {
            var emp = await _employeeRepository.GetByIdAsync(leave.EmployeeId);
            if (emp != null)
                result.Add(MapToDto(leave, emp));
        }

        return result;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetAllLeavesAsync()
    {
        var all = await _leaveRepository.GetAllAsync();
        var allLeaves = all.Where(l => !l.IsDeleted).ToList();

        var result = new List<LeaveRequestDto>();
        foreach (var leave in allLeaves)
        {
            var emp = await _employeeRepository.GetByIdAsync(leave.EmployeeId);
            if (emp != null)
                result.Add(MapToDto(leave, emp));
        }

        return result;
    }

    public async Task<IEnumerable<LeaveRequestDto>> GetPendingLeavesAsync()
    {
        var all = await _leaveRepository.GetAllAsync();
        var pendingLeaves = all.Where(l =>
            (l.Status == LeaveStatus.PendingManagerApproval || l.Status == LeaveStatus.PendingHRApproval)
            && !l.IsDeleted).ToList();

        var result = new List<LeaveRequestDto>();
        foreach (var leave in pendingLeaves)
        {
            var emp = await _employeeRepository.GetByIdAsync(leave.EmployeeId);
            if (emp != null)
                result.Add(MapToDto(leave, emp));
        }

        return result;
    }

    public async Task<bool> ApproveRequestAsync(int requestId, int approverId, string approverName, string callerRole, string? adminMessage = null)
    {
        var request = await _leaveRepository.GetByIdAsync(requestId);
        if (request == null || request.IsDeleted) return false;

        string decision;

        if (callerRole == "Manager")
        {
            // Manager can only act on PendingManagerApproval
            if (request.Status != LeaveStatus.PendingManagerApproval)
                throw new InvalidOperationException("This request is not awaiting Manager approval.");

            request.Status = LeaveStatus.PendingHRApproval;
            request.AdminMessage = adminMessage;
            decision = "Approved by Manager";
        }
        else if (callerRole == "Admin")
        {
            // Admin (HR) can act on PendingHRApproval
            if (request.Status != LeaveStatus.PendingHRApproval)
                throw new InvalidOperationException("This request is not awaiting HR approval. It must be approved by a Manager first.");

            // Deduct leave balance for applicable leave types
            if (request.Type == LeaveType.Annual || request.Type == LeaveType.Emergency)
            {
                var year = request.StartDate.Year;
                var balance = await _leaveBalanceRepository.GetOrCreateAsync(request.EmployeeId, year);

                // Calculate working days between start and end (inclusive)
                int daysRequested = ((request.EndDate.ToDateTime(TimeOnly.MinValue) - request.StartDate.ToDateTime(TimeOnly.MinValue)).Days) + 1;

                if (request.Type == LeaveType.Annual)
                {
                    if (balance.AnnualLeaveRemaining < daysRequested)
                        throw new InvalidOperationException($"Insufficient Annual Leave (RDO) balance. Employee has {balance.AnnualLeaveRemaining} day(s) remaining but requested {daysRequested} day(s).");

                    balance.AnnualLeaveUsed += daysRequested;
                }
                else if (request.Type == LeaveType.Emergency)
                {
                    if (balance.EmergencyLeaveRemaining < daysRequested)
                        throw new InvalidOperationException($"Insufficient Emergency Leave (EDO) balance. Employee has {balance.EmergencyLeaveRemaining} day(s) remaining but requested {daysRequested} day(s).");

                    balance.EmergencyLeaveUsed += daysRequested;
                }

                balance.UpdatedAt = DateTime.UtcNow;
                _leaveBalanceRepository.Update(balance);
            }

            request.Status = LeaveStatus.Approved;
            request.AdminMessage = adminMessage;
            request.ApprovedById = null; // FK bypass as per existing pattern
            decision = "Approved by HR";
        }
        else
        {
            return false;
        }

        request.UpdatedAt = DateTime.UtcNow;
        _leaveRepository.Update(request);

        // Record approval history
        var history = new ApprovalHistory
        {
            RequestType = "Leave",
            RequestId = requestId,
            ApproverId = approverId,
            ApproverName = approverName,
            ApproverRole = callerRole,
            Decision = decision,
            Comment = adminMessage,
            DecisionAt = DateTime.UtcNow
        };
        await _approvalHistoryRepository.AddAsync(history);

        await _leaveRepository.SaveChangesAsync();

        // Send notification email
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee != null && !string.IsNullOrEmpty(employee.Email))
        {
            var isFullyApproved = request.Status == LeaveStatus.Approved;
            var subject = isFullyApproved ? "Leave Request Approved" : "Leave Request – Manager Approved (Pending HR)";
            var emailBody = isFullyApproved
                ? $"Hello {employee.FirstName} {employee.LastName},\n\nYour {FormatLeaveType(request.Type)} leave request from {request.StartDate} to {request.EndDate} has been fully approved by HR.\n"
                : $"Hello {employee.FirstName} {employee.LastName},\n\nYour {FormatLeaveType(request.Type)} leave request from {request.StartDate} to {request.EndDate} has been approved by your Manager and is now pending final HR approval.\n";

            if (!string.IsNullOrEmpty(adminMessage))
                emailBody += $"\nNote: {adminMessage}\n";
            emailBody += "\nBest Regards,\nHR Team";

            _ = Task.Run(async () =>
            {
                try { await _emailService.SendEmailAsync(employee.Email!, subject, emailBody); } catch { }
            });
        }

        return true;
    }

    public async Task<bool> RejectRequestAsync(int requestId, int approverId, string approverName, string callerRole, string? adminMessage = null)
    {
        var request = await _leaveRepository.GetByIdAsync(requestId);
        if (request == null || request.IsDeleted) return false;

        // Validate that the caller has permission to reject at the current stage
        if (callerRole == "Manager" && request.Status != LeaveStatus.PendingManagerApproval)
            throw new InvalidOperationException("This request is not awaiting Manager approval.");

        if (callerRole == "Admin" && request.Status != LeaveStatus.PendingHRApproval && request.Status != LeaveStatus.PendingManagerApproval)
            throw new InvalidOperationException("This request cannot be rejected at its current stage.");

        request.Status = LeaveStatus.Rejected;
        request.AdminMessage = adminMessage;
        request.UpdatedAt = DateTime.UtcNow;
        _leaveRepository.Update(request);

        // Record rejection history
        var history = new ApprovalHistory
        {
            RequestType = "Leave",
            RequestId = requestId,
            ApproverId = approverId,
            ApproverName = approverName,
            ApproverRole = callerRole,
            Decision = "Rejected",
            Comment = adminMessage,
            DecisionAt = DateTime.UtcNow
        };
        await _approvalHistoryRepository.AddAsync(history);

        await _leaveRepository.SaveChangesAsync();

        // Send rejection email
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId);
        if (employee != null && !string.IsNullOrEmpty(employee.Email))
        {
            var emailBody = $"Hello {employee.FirstName} {employee.LastName},\n\nUnfortunately, your {FormatLeaveType(request.Type)} leave request from {request.StartDate} to {request.EndDate} has been rejected by {callerRole}.\n";
            if (!string.IsNullOrEmpty(adminMessage))
                emailBody += $"\nNote: {adminMessage}\n";
            emailBody += "\nBest Regards,\nHR Team";

            _ = Task.Run(async () =>
            {
                try { await _emailService.SendEmailAsync(employee.Email!, "Leave Request Rejected", emailBody); } catch { }
            });
        }

        return true;
    }

    public async Task<LeaveBalanceDto> GetLeaveBalanceAsync(int employeeId, int year)
    {
        var balance = await _leaveBalanceRepository.GetOrCreateAsync(employeeId, year);
        return new LeaveBalanceDto
        {
            EmployeeId = balance.EmployeeId,
            Year = balance.Year,
            AnnualLeaveTotal = balance.AnnualLeaveTotal,
            AnnualLeaveUsed = balance.AnnualLeaveUsed,
            AnnualLeaveRemaining = balance.AnnualLeaveRemaining,
            EmergencyLeaveTotal = balance.EmergencyLeaveTotal,
            EmergencyLeaveUsed = balance.EmergencyLeaveUsed,
            EmergencyLeaveRemaining = balance.EmergencyLeaveRemaining
        };
    }

    public async Task<LeaveBalanceDto> UpdateLeaveBalanceAsync(int employeeId, int year, UpdateLeaveBalanceDto dto)
    {
        var balance = await _leaveBalanceRepository.GetOrCreateAsync(employeeId, year);
        
        balance.AnnualLeaveTotal = dto.AnnualLeaveTotal;
        balance.EmergencyLeaveTotal = dto.EmergencyLeaveTotal;
        balance.UpdatedAt = DateTime.UtcNow;

        _leaveBalanceRepository.Update(balance);
        await _leaveBalanceRepository.SaveChangesAsync();

        return new LeaveBalanceDto
        {
            EmployeeId = balance.EmployeeId,
            Year = balance.Year,
            AnnualLeaveTotal = balance.AnnualLeaveTotal,
            AnnualLeaveUsed = balance.AnnualLeaveUsed,
            AnnualLeaveRemaining = balance.AnnualLeaveRemaining,
            EmergencyLeaveTotal = balance.EmergencyLeaveTotal,
            EmergencyLeaveUsed = balance.EmergencyLeaveUsed,
            EmergencyLeaveRemaining = balance.EmergencyLeaveRemaining
        };
    }

    public async Task<IEnumerable<ApprovalHistoryDto>> GetApprovalHistoryAsync(int requestId, string requestType)
    {
        var allHistory = await _approvalHistoryRepository.GetAllAsync();
        return allHistory
            .Where(h => h.RequestId == requestId && h.RequestType == requestType && !h.IsDeleted)
            .OrderBy(h => h.DecisionAt)
            .Select(h => new ApprovalHistoryDto
            {
                Id = h.Id,
                RequestType = h.RequestType,
                RequestId = h.RequestId,
                ApproverId = h.ApproverId,
                ApproverName = h.ApproverName,
                ApproverRole = h.ApproverRole,
                Decision = h.Decision,
                Comment = h.Comment,
                DecisionAt = h.DecisionAt
            });
    }

    public async Task<bool> DeleteRequestAsync(int requestId, int employeeId, bool isAdmin)
    {
        var request = await _leaveRepository.GetByIdAsync(requestId);
        if (request == null || request.IsDeleted) return false;

        if (!isAdmin && request.EmployeeId != employeeId) return false;

        _leaveRepository.Delete(request);
        await _leaveRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllEmployeeRequestsAsync(int employeeId)
    {
        var all = await _leaveRepository.GetAllAsync();
        var employeeLeaves = all.Where(l => l.EmployeeId == employeeId && !l.IsDeleted).ToList();

        foreach (var leave in employeeLeaves)
        {
            _leaveRepository.Delete(leave);
        }
        await _leaveRepository.SaveChangesAsync();
        return true;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private LeaveRequestDto MapToDto(LeaveRequest leave, Employee employee)
    {
        return new LeaveRequestDto
        {
            Id = leave.Id,
            EmployeeId = leave.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            LeaveType = FormatLeaveType(leave.Type),
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            Reason = leave.Reason,
            Status = leave.Status.ToString(),
            StatusLabel = GetStatusLabel(leave.Status),
            AdminMessage = leave.AdminMessage,
            CreatedAt = leave.CreatedAt
        };
    }

    private static string FormatLeaveType(LeaveType type) => type switch
    {
        LeaveType.Annual    => "Annual Leave (RDO)",
        LeaveType.Emergency => "Emergency Leave (EDO)",
        LeaveType.Sick      => "Reported Sick Day (RSD)",
        LeaveType.Absence   => "Absence Without Permission (AWD)",
        _                   => type.ToString()
    };

    private static string GetStatusLabel(LeaveStatus status) => status switch
    {
        LeaveStatus.PendingManagerApproval => "Pending Manager Approval",
        LeaveStatus.PendingHRApproval      => "Pending HR Approval",
        LeaveStatus.Approved               => "Approved",
        LeaveStatus.Rejected               => "Rejected",
        _                                  => status.ToString()
    };
}
