// Models/Task.cs
using System;

namespace Volunteers.Models
{
    public class Task
    {
        public int TaskID { get; set; }
        public int ProjectID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
