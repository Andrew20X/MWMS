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

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateOnly HireDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Foreign Keys
    public int DepartmentId { get; set; }

    public int PositionId { get; set; }

    public int ShiftId { get; set; }

    // Navigation Properties
    public Department Department { get; set; } = null!;

    public Position Position { get; set; } = null!;

    public Shift Shift { get; set; } = null!;

    [JsonIgnore]
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
