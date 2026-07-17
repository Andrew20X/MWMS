using MWMS.Application.Interfaces;
using MWMS.Application.Services;
using MWMS.Domain.Entities;

namespace MWMS.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;

    public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _employeeRepository.GetAllAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _employeeRepository.GetByIdAsync(id);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        var validationError = await _employeeRepository.ValidateReferencesAsync(
            employee.DepartmentId,
            employee.PositionId,
            employee.ShiftId);

        if (validationError is not null)
            throw new InvalidOperationException(validationError);

        var existingByCode = await _employeeRepository.GetByEmployeeCodeAsync(employee.EmployeeCode);
        if (existingByCode is not null)
            throw new InvalidOperationException($"Employee code '{employee.EmployeeCode}' is already in use.");

        await _employeeRepository.AddAsync(employee);
        await _employeeRepository.SaveChangesAsync();
        return employee;
    }

    public async Task<Employee?> UpdateAsync(int id, Employee employee)
    {
        var existing = await _employeeRepository.GetByIdAsync(id);
        if (existing is null)
            return null;

        existing.EmployeeCode = employee.EmployeeCode;
        existing.DeviceUserId = employee.DeviceUserId;
        existing.FirstName = employee.FirstName;
        existing.MiddleName = employee.MiddleName;
        existing.LastName = employee.LastName;
        existing.Email = employee.Email;
        existing.Phone = employee.Phone;
        existing.DepartmentId = employee.DepartmentId;
        existing.PositionId = employee.PositionId;
        existing.ShiftId = employee.ShiftId;
        existing.IsActive = employee.IsActive;

        _employeeRepository.Update(existing);
        
        // Also sync the changes to the user account if it exists
        if (!string.IsNullOrEmpty(existing.EmployeeCode))
        {
            var username = $"EMP-SYNC-{existing.EmployeeCode}";
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null)
            {
                user.Email = existing.Email ?? "";
                user.FullName = $"{existing.FirstName} {existing.LastName}";
                _userRepository.Update(user);
            }
        }

        await _employeeRepository.SaveChangesAsync();
        await _userRepository.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _employeeRepository.GetByIdAsync(id);
        if (existing is null)
            return false;

        _employeeRepository.Delete(existing);

        if (!string.IsNullOrEmpty(existing.EmployeeCode))
        {
            var username = $"EMP-SYNC-{existing.EmployeeCode}";
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user != null)
            {
                _userRepository.Delete(user);
            }
        }

        await _employeeRepository.SaveChangesAsync();
        await _userRepository.SaveChangesAsync();
        return true;
    }
}
