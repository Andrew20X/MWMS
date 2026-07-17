namespace MWMS.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalEmployees { get; set; }
    public int PresentToday { get; set; }
    public int LateArrivals { get; set; }
    public int Absent { get; set; }
}
