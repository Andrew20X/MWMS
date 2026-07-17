using System;
using MWMS.Domain.Common;

namespace MWMS.Domain.Entities;

public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Notice"; // "Notice" or "Holiday"
    public DateOnly? TargetDate { get; set; } // For holidays
    public bool IsActive { get; set; } = true;
}
