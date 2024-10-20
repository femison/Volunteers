// Models/UserTask.cs
namespace Volunteers.Models
{
    public class UserTask
    {
        public int UserTaskID { get; set; }
        public int UserID { get; set; }
        public int TaskID { get; set; }
        public int ProjectID { get; set; }
    }
}
