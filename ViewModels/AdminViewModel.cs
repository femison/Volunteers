// ViewModels/AdminViewModel.cs
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;

namespace Volunteers.ViewModels
{
    public class AdminViewModel
    {
        public List<User> Users { get; set; }
        public List<Project> Projects { get; set; }
        public List<TaskModel> Tasks { get; set; }
        public List<UserTaskAssignment> UserTasks { get; set; }

        public AdminViewModel()
        {
            Users = new List<User>();
            Projects = new List<Project>();
            Tasks = new List<TaskModel>();
            UserTasks = new List<UserTaskAssignment>();
        }

        public void LoadUsersData()
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT UserID, Name, Surname, UserSkills, Email, Phone, DateOfBirth, Gender, Address, Role FROM users";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Users.Clear();
                    while (reader.Read())
                    {
                        Users.Add(new User
                        {
                            UserID = reader.GetInt32("UserID"),
                            Name = reader.GetString("Name"),
                            Surname = reader.GetString("Surname"),
                            UserSkills = reader.GetString("UserSkills"),
                            Email = reader.GetString("Email"),
                            Phone = reader.GetString("Phone"),
                            DateOfBirth = reader.GetDateTime("DateOfBirth"),
                            Gender = reader.GetString("Gender"),
                            Address = reader.GetString("Address"),
                            Role = reader.GetString("Role")
                        });
                    }
                }
            }
        }

        public void LoadProjectsData()
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT ProjectID, ProjectName, StartDate, EndDate, Status FROM projects";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Projects.Clear();
                    while (reader.Read())
                    {
                        Projects.Add(new Project
                        {
                            ProjectID = reader.GetInt32("ProjectID"),
                            ProjectName = reader.GetString("ProjectName"),
                            StartDate = reader.GetDateTime("StartDate"),
                            EndDate = reader.GetDateTime("EndDate"),
                            Status = reader.GetString("Status")
                        });
                    }
                }
            }
        }

        public void LoadTasksData()
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT t.TaskID, p.ProjectName, t.Description, t.Status, ti.Location, ti.Date
                                 FROM tasks t
                                 JOIN projects p ON t.ProjectID = p.ProjectID
                                 JOIN taskinfo ti ON t.TaskID = ti.TaskID";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Tasks.Clear();
                    while (reader.Read())
                    {
                        Tasks.Add(new TaskModel
                        {
                            TaskID = reader.GetInt32("TaskID"),
                            ProjectName = reader.GetString("ProjectName"),
                            Description = reader.GetString("Description"),
                            Status = reader.GetString("Status"),
                            Location = reader.GetString("Location"),
                            Date = reader.GetDateTime("Date")
                        });
                    }
                }
            }
        }

        public void LoadUserTasksData()
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                connection.Open();
                string query = @"SELECT ut.UserTaskID, CONCAT(u.Name, ' ', u.Surname) AS FullName, t.Description, p.ProjectName
                                 FROM user_tasks ut
                                 JOIN users u ON ut.UserID = u.UserID
                                 JOIN tasks t ON ut.TaskID = t.TaskID
                                 JOIN projects p ON ut.ProjectID = p.ProjectID";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    UserTasks.Clear();
                    while (reader.Read())
                    {
                        UserTasks.Add(new UserTaskAssignment
                        {
                            UserTaskID = reader.GetInt32("UserTaskID"),
                            FullName = reader.GetString("FullName"),
                            Description = reader.GetString("Description"),
                            ProjectName = reader.GetString("ProjectName")
                        });
                    }
                }
            }
        }

        public List<T> FilterData<T>(List<T> data, string query)
        {
            return data.Where(item => item.ToString().ToLower().Contains(query.ToLower())).ToList();
        }
    }
}
