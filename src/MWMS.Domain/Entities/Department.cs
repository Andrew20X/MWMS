using System.Text.Json.Serialization;
using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation Property
    [JsonIgnore]
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}