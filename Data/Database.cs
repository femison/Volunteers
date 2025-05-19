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

        // Получить все заявки
        public static List<RequestModel> GetAllRequests()
        {
            var requests = new List<RequestModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                string query = @"
            SELECT 
                upa.RequestID,
                u.UserID,
                u.Name,
                u.Surname,
                u.UserSkills,
                u.Email,
                u.Phone,
                u.Gender,
                u.DateOfBirth,
                u.Address,

                p.ProjectID,
                p.ProjectName,
                p.StartDate AS ProjectStart,
                p.EndDate AS ProjectEnd,
                p.Status AS ProjectStatus,

                t.TaskID,
                t.Description AS TaskDescription,
                t.Status AS TaskStatus,

                ti.Location,
                ti.Date AS TaskDate,

                upa.Status,
                upa.CreatedAt,
                upa.UpdatedAt

            FROM users_pending_approval upa
            JOIN users u ON upa.UserID = u.UserID
            JOIN tasks t ON upa.TaskID = t.TaskID
            JOIN taskinfo ti ON t.TaskID = ti.TaskID
            JOIN projects p ON t.ProjectID = p.ProjectID

            WHERE u.Role = 'Волонтер'
            ORDER BY upa.CreatedAt DESC;
        ";

                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var request = new RequestModel
                        {
                         
                        };

                        requests.Add(request);
                    }
                }
            }

            return requests;
        }


        // Одобрить заявку
        public static void ApproveRequest(int requestId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                // Получить заявку
                string selectQuery = "SELECT * FROM users_pending_approval WHERE RequestID = @RequestID";
                MySqlCommand selectCmd = new MySqlCommand(selectQuery, connection);
                selectCmd.Parameters.AddWithValue("@RequestID", requestId);

                int userId = 0, taskId = 0, projectId = 0;

                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userId = reader.GetInt32("UserID");
                        projectId = reader.GetInt32("ProjectID");
                        taskId = reader.GetInt32("TaskID");
                    }
                }

                if (userId > 0)
                {
                    // Добавить в user_tasks
                    AddUserTaskAssignment(userId, taskId, projectId);

                    // Удалить заявку
                    string deleteQuery = "DELETE FROM users_pending_approval WHERE RequestID = @RequestID";
                    MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection);
                    deleteCmd.Parameters.AddWithValue("@RequestID", requestId);
                    deleteCmd.ExecuteNonQuery();
                }
            }
        }

        // Отклонить заявку
        public static void RejectRequest(int requestId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM users_pending_approval WHERE RequestID = @RequestID";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RequestID", requestId);
                cmd.ExecuteNonQuery();
            }
        }


    }
}
