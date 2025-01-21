// Models/UserTask.cs
namespace Volunteers.Models
{
    public class UserTask
    {
        public int UserTaskID { get; set; }
        public int UserID { get; set; }
        public int TaskID { get; set; }
        public int ProjectID { get; set; }

        // Навигационные свойства
        public User User { get; set; }
        public TaskModel Task { get; set; }
        public Project Project { get; set; }
    }
}