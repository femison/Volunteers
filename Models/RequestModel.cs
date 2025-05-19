using System;

namespace Volunteers.Models
{
    public class RequestModel
    {
        public int RequestID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ProjectName { get; set; }
        public string TaskDescription { get; set; }
        public string Status { get; set; } // статус заявки
    }
}