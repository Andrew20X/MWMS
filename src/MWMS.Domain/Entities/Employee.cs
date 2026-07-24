using System.Text.Json.Serialization;
using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;

    public int DeviceUserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateOnly HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Foreign Keys
    public int DepartmentId { get; set; }

    public int PositionId { get; set; }

    public int ShiftId { get; set; }

    /// <summary>Self-referencing FK to the employee's direct manager.</summary>
    public int? ManagerId { get; set; }

    // Navigation Properties
    public Department Department { get; set; } = null!;

    public Position Position { get; set; } = null!;

    public Shift Shift { get; set; } = null!;

    /// <summary>The employee's direct manager.</summary>
    [JsonIgnore]
    public Employee? Manager { get; set; }

    /// <summary>Employees who report to this employee.</summary>
    [JsonIgnore]
    public ICollection<Employee> Subordinates { get; set; } = new List<Employee>();

    [JsonIgnore]
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? Role { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? ManagerName { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<int> SubordinateIds { get; set; } = new List<int>();

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? SubordinatesList { get; set; }
}
