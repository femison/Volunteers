using MySql.Data.MySqlClient;
using Volunteers.Models;

namespace Volunteers.Data
{
    public static class Database
    {
        private static string connectionString = "Server=localhost;Database=voli;Uid=root;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // Получение задач с данными проекта
        public static List<TaskModel> GetTasksWithProjectName()
        {
            var tasks = new List<TaskModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"SELECT t.TaskID, p.ProjectName, t.Description, t.Status, ti.Location, ti.Date 
                                 FROM tasks t
                                 JOIN projects p ON t.ProjectID = p.ProjectID
                                 JOIN taskinfo ti ON t.TaskID = ti.TaskID";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var task = new TaskModel
                        {
                            TaskID = reader.GetInt32("TaskID"),
                            ProjectName = reader.GetString("ProjectName"),
                            Description = reader.GetString("Description"),
                            Status = reader.GetString("Status"),
                            Location = reader.GetString("Location"),
                            Date = reader.GetDateTime("Date")
                        };
                        tasks.Add(task);
                    }
                }
            }

            return tasks;
        }

        // Добавление участия в таблицу user_tasks
        public static void AddUserTaskAssignment(int userId, int taskId, int projectId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO user_tasks (UserID, TaskID, ProjectID) VALUES (@UserID, @TaskID, @ProjectID)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@ProjectID", projectId);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
