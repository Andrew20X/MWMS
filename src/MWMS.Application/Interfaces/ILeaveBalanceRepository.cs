using MWMS.Domain.Entities;

namespace MWMS.Application.Interfaces;

/// <summary>
/// Repository for reading and writing per-employee, per-year leave balances.
/// </summary>
public interface ILeaveBalanceRepository : IGenericRepository<LeaveBalance>
{
    /// <summary>Returns the balance record for the given employee and year, or null if not yet created.</summary>
    Task<LeaveBalance?> GetByEmployeeAndYearAsync(int employeeId, int year);

    /// <summary>
    /// Returns the balance record for the given employee and year.
    /// If none exists, creates one with default quotas (Annual=15, Emergency=6) and persists it.
    /// </summary>
    Task<LeaveBalance> GetOrCreateAsync(int employeeId, int year);
}
