// Models/TaskModel.cs
using System;

namespace Volunteers.Models
{
    public class TaskModel
    {
        public int TaskID { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
    }
}
