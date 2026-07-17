using MWMS.Domain.Enums;

namespace MWMS.Application.DTOs.Attendance;

public class AttendanceFilterDto
{
    public DateOnly? StartDate { get; set; }
    
    public DateOnly? EndDate { get; set; }
    
    public string? EmployeeName { get; set; }
    
    public string? EmployeeCode { get; set; }
    
    public int? DepartmentId { get; set; }
    
    public AttendanceStatus? Status { get; set; }
}
