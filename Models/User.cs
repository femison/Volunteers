// Models/User.cs
using System;

namespace Volunteers.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserSkills { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }

        // Свойство для отображения ФИО, только для чтения
        public string FullName => $"{Name} {Surname}";
    }


}
