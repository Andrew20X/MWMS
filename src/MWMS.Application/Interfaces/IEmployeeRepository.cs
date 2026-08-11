using MWMS.Domain.Entities;

namespace MWMS.Application.Interfaces;

public interface IEmployeeRepository : IGenericRepository<Employee>
{
    Task<Employee?> GetByEmployeeCodeAsync(string employeeCode);

    Task<Employee?> GetByDeviceUserIdAsync(int deviceUserId);

    Task<Employee?> GetByEmailAsync(string email);

    Task<string?> ValidateReferencesAsync(int departmentId, int positionId, int shiftId);

    Task<IEnumerable<Employee>> GetByManagerIdAsync(int managerId);

    Task<IEnumerable<Employee>> GetActiveEmployeesBasicAsync();
}