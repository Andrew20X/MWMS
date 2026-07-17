namespace MWMS.Application.DTOs;

public class SyncEmployeeDto
{
    public int DeviceUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string DeviceId { get; set; } = string.Empty;
}
