// Models/UserPendingApproval.cs
using System;

namespace Volunteers.Models
{
    public class UserPendingApproval
    {
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public int ProjectID { get; set; }
        public int TaskID { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
