using System.Text.Json.Serialization;
using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class Shift : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int GraceMinutes { get; set; }

    public int LunchMinutes { get; set; }

    // Navigation Property
    [JsonIgnore]
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}