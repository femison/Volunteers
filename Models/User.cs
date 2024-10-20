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
        public string Gender { get; set; } // 'м' или 'ж'
        public string Address { get; set; }
        public string Role { get; set; } // 'Волонтер' или 'Администратор'

        public string FullName => $"{Name} {Surname}";
    }
}
