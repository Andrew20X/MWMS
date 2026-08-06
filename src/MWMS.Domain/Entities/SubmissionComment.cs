using MWMS.Domain.Common;

namespace MWMS.Domain.Entities
{
    public class SubmissionComment : BaseEntity
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public string FileName { get; set; }
        public string CommentText { get; set; }
    }
}
