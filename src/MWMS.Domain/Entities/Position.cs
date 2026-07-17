using System.Text.Json.Serialization;
using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class Position : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    // Navigation Property
    [JsonIgnore]
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}