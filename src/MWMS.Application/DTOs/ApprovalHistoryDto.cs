namespace MWMS.Application.DTOs;

public class ApprovalHistoryDto
{
    public int Id { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public int RequestId { get; set; }
    public int ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime DecisionAt { get; set; }
}
