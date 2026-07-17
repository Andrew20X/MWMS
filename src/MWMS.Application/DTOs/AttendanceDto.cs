namespace MWMS.Application.DTOs;

public class AttendanceDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public DateOnly Date { get; set; }

    public TimeOnly? CheckIn { get; set; }

    public TimeOnly? CheckOut { get; set; }

    public string Status { get; set; } = string.Empty;

    public double WorkedHours { get; set; }

    public int LateMinutes { get; set; }

    public int EarlyLeaveMinutes { get; set; }

    public int OvertimeMinutes { get; set; }

    public string? Notes { get; set; }
}