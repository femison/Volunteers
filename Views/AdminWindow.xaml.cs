using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;

namespace Volunteers.Views
{
    public partial class AdminWindow : Window
    {
        private User currentUser;
        private List<User> usersList;
        private List<Project> projectsList;
        private List<TaskModel> tasksList;
        private List<RequestModel> requestsList;

        public AdminWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            LoadUsersData();
            LoadProjectsData();
            LoadTasksData();
            LoadAccounts();
            LoadUserTasks();
            LoadRequestsData();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            LoadUsersData();
            LoadProjectsData();
            LoadTasksData();
            LoadAccounts();
            LoadUserTasks();
            LoadRequestsData();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.Owner != null)
            {
                MainWindow mainWindow = this.Owner as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ClearLoginFields();
                    mainWindow.Show();
                }
            }
        }

        #region Управление Пользователями

        private void LoadUsersData()
        {
            usersList = new List<User>();
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT u.UserID, u.Name, u.Surname, u.UserSkills, u.Email, u.Phone, 
                                     u.DateOfBirth, u.Gender, u.Address, u.Role 
                                     FROM users u";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usersList.Add(new User
                            {
                                UserID = reader.GetInt32("UserID"),
                                Name = reader.GetString("Name"),
                                Surname = reader.GetString("Surname"),
                                UserSkills = reader.IsDBNull(reader.GetOrdinal("UserSkills")) ? "" : reader.GetString("UserSkills"),
                                Email = reader.GetString("Email"),
                                Phone = reader.GetString("Phone"),
                                DateOfBirth = reader.GetDateTime("DateOfBirth"),
                                Gender = reader.GetString("Gender"),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString("Address"),
                                Role = reader.GetString("Role")
                            });
                        }
                    }
                    UsersDataGrid.ItemsSource = usersList;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки данных пользователей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUserWindow = new AddUserWindow();
            addUserWindow.Owner = this;
            if (addUserWindow.ShowDialog() == true)
            {
                LoadUsersData();
            }
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = Convert.ToInt32((sender as Button).Tag);
            User selectedUser = usersList.Find(u => u.UserID == userId);
            if (selectedUser != null)
            {
                EditUserWindow editWindow = new EditUserWindow(selectedUser);
                editWindow.Owner = this;
                if (editWindow.ShowDialog() == true)
                {
                    LoadUsersData();
                }
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = Convert.ToInt32((sender as Button).Tag);
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (DeleteUserFromDatabase(userId))
                {
                    MessageBox.Show("Пользователь успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsersData();
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool DeleteUserFromDatabase(int userId)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();
                    string deleteCredentialQuery = "DELETE FROM usercredentials WHERE UserID=@UserID";
                    MySqlCommand cmdCredential = new MySqlCommand(deleteCredentialQuery, connection, transaction);
                    cmdCredential.Parameters.AddWithValue("@UserID", userId);
                    cmdCredential.ExecuteNonQuery();
                    string deleteUserQuery = "DELETE FROM users WHERE UserID=@UserID";
                    MySqlCommand cmdUser = new MySqlCommand(deleteUserQuery, connection, transaction);
                    cmdUser.Parameters.AddWithValue("@UserID", userId);
                    cmdUser.ExecuteNonQuery();
                    transaction.Commit();
                    return true;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при удалении пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private void UserSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = UserSearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadUsersData();
                return;
            }
            try
            {
                using (MySqlConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT u.UserID, u.Name, u.Surname, u.UserSkills, u.Email, u.Phone, 
                                     u.DateOfBirth, u.Gender, u.Address, u.Role 
                                     FROM users u 
                                     WHERE u.Name LIKE @Search 
                                     OR u.Surname LIKE @Search 
                                     OR u.Email LIKE @Search 
                                     OR u.Role LIKE @Search";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Search", "%" + searchQuery + "%");
                    List<User> filteredUsers = new List<User>();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            filteredUsers.Add(new User
                            {
                                UserID = reader.GetInt32("UserID"),
                                Name = reader.GetString("Name"),
                                Surname = reader.GetString("Surname"),
                                UserSkills = reader.IsDBNull(reader.GetOrdinal("UserSkills")) ? "" : reader.GetString("UserSkills"),
                                Email = reader.GetString("Email"),
                                Phone = reader.GetString("Phone"),
                                DateOfBirth = reader.GetDateTime("DateOfBirth"),
                                Gender = reader.GetString("Gender"),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString("Address"),
                                Role = reader.GetString("Role")
                            });
                        }
                    }
                    UsersDataGrid.ItemsSource = filteredUsers;
                    if (filteredUsers.Count == 0)
                    {
                        MessageBox.Show("Ни одного пользователя не найдено по заданному запросу.", "Результаты поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Ошибка при поиске пользователей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserResetButton_Click(object sender, RoutedEventArgs e)
        {
            UserSearchTextBox.Clear();
            LoadUsersData();
        }

        private void UserSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UserSearchButton_Click(sender, e);
            }
        }

        #endregion

        #region Управление Проектами

        private void LoadProjectsData()
        {
            projectsList = new List<Project>();
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT ProjectID, ProjectName, StartDate, EndDate, Status FROM projects";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            projectsList.Add(new Project
                            {
                                ProjectID = reader.GetInt32("ProjectID"),
                                ProjectName = reader.GetString("ProjectName"),
                                StartDate = reader.GetDateTime("StartDate"),
                                EndDate = reader.GetDateTime("EndDate"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                    ProjectsDataGrid.ItemsSource = projectsList;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки данных проектов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            LoadTasksData();
        }

        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            AddProjectWindow addProjectWindow = new AddProjectWindow();
            addProjectWindow.Owner = this;
            if (addProjectWindow.ShowDialog() == true)
            {
                LoadProjectsData();
            }
        }

        private void EditProjectButton_Click(object sender, RoutedEventArgs e)
        {
            int projectId = Convert.ToInt32((sender as Button).Tag);
            Project selectedProject = projectsList.Find(p => p.ProjectID == projectId);
            if (selectedProject != null)
            {
                EditProjectWindow editWindow = new EditProjectWindow(selectedProject);
                editWindow.Owner = this;
                if (editWindow.ShowDialog() == true)
                {
                    LoadProjectsData();
                }
            }
        }

        private void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            int projectId = Convert.ToInt32((sender as Button).Tag);
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить этот проект?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (DeleteProjectFromDatabase(projectId))
                {
                    MessageBox.Show("Проект успешно удален.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProjectsData();
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool DeleteProjectFromDatabase(int projectId)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();
                    string deleteTasksQuery = "DELETE FROM tasks WHERE ProjectID=@ProjectID";
                    MySqlCommand cmdTasks = new MySqlCommand(deleteTasksQuery, connection, transaction);
                    cmdTasks.Parameters.AddWithValue("@ProjectID", projectId);
                    cmdTasks.ExecuteNonQuery();
                    string deleteProjectQuery = "DELETE FROM projects WHERE ProjectID=@ProjectID";
                    MySqlCommand cmdProject = new MySqlCommand(deleteProjectQuery, connection, transaction);
                    cmdProject.Parameters.AddWithValue("@ProjectID", projectId);
                    cmdProject.ExecuteNonQuery();
                    transaction.Commit();
                    return true;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при удалении проекта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private void ProjectSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = ProjectSearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadProjectsData();
                return;
            }
            try
            {
                using (MySqlConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT ProjectID, ProjectName, StartDate, EndDate, Status 
                                     FROM projects 
                                     WHERE ProjectName LIKE @Search OR Status LIKE @Search";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Search", "%" + searchQuery + "%");
                    List<Project> filteredProjects = new List<Project>();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            filteredProjects.Add(new Project
                            {
                                ProjectID = reader.GetInt32("ProjectID"),
                                ProjectName = reader.GetString("ProjectName"),
                                StartDate = reader.GetDateTime("StartDate"),
                                EndDate = reader.GetDateTime("EndDate"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                    ProjectsDataGrid.ItemsSource = filteredProjects;
                    if (filteredProjects.Count == 0)
                    {
                        MessageBox.Show("Ни одного проекта не найдено по заданному запросу.", "Результаты поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Ошибка при поиске проектов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProjectResetButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectSearchTextBox.Clear();
            LoadProjectsData();
        }

        private void ProjectSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProjectSearchButton_Click(sender, e);
            }
        }

        #endregion

        #region Управление Задачами

        private void LoadTasksData()
        {
            tasksList = new List<TaskModel>();
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT t.TaskID, p.ProjectName, t.Description, t.Status, ti.Location, ti.Date
                                     FROM tasks t
                                     JOIN projects p ON t.ProjectID = p.ProjectID
                                     JOIN taskinfo ti ON t.TaskID = ti.TaskID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasksList.Add(new TaskModel
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
                    TasksDataGrid.ItemsSource = tasksList;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки данных задач: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            AddTaskWindow addTaskWindow = new AddTaskWindow(projectsList);
            addTaskWindow.Owner = this;
            if (addTaskWindow.ShowDialog() == true)
            {
                LoadTasksData();
            }
        }

        private void EditTaskButton_Click(object sender, RoutedEventArgs e)
        {
            int taskId = Convert.ToInt32((sender as Button).Tag);
            TaskModel selectedTask = tasksList.Find(t => t.TaskID == taskId);
            if (selectedTask != null)
            {
                EditTaskWindow editTaskWindow = new EditTaskWindow(selectedTask, projectsList);
                editTaskWindow.Owner = this;
                if (editTaskWindow.ShowDialog() == true)
                {
                    LoadTasksData();
                }
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            int taskId = Convert.ToInt32((sender as Button).Tag);
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить эту задачу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                if (DeleteTaskFromDatabase(taskId))
                {
                    MessageBox.Show("Задача успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadTasksData();
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении задачи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool DeleteTaskFromDatabase(int taskId)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();
                    string deleteUserTasksQuery = "DELETE FROM user_tasks WHERE TaskID=@TaskID";
                    MySqlCommand cmdUserTasks = new MySqlCommand(deleteUserTasksQuery, connection, transaction);
                    cmdUserTasks.Parameters.AddWithValue("@TaskID", taskId);
                    cmdUserTasks.ExecuteNonQuery();
                    string deleteTaskInfoQuery = "DELETE FROM taskinfo WHERE TaskID=@TaskID";
                    MySqlCommand cmdTaskInfo = new MySqlCommand(deleteTaskInfoQuery, connection, transaction);
                    cmdTaskInfo.Parameters.AddWithValue("@TaskID", taskId);
                    cmdTaskInfo.ExecuteNonQuery();
                    string deleteTaskQuery = "DELETE FROM tasks WHERE TaskID=@TaskID";
                    MySqlCommand cmdTask = new MySqlCommand(deleteTaskQuery, connection, transaction);
                    cmdTask.Parameters.AddWithValue("@TaskID", taskId);
                    cmdTask.ExecuteNonQuery();
                    transaction.Commit();
                    return true;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при удалении задачи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private void TaskSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = TaskSearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadTasksData();
                return;
            }
            try
            {
                using (MySqlConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT t.TaskID, p.ProjectName, t.Description, t.Status, ti.Location, ti.Date
                                     FROM tasks t
                                     JOIN projects p ON t.ProjectID = p.ProjectID
                                     JOIN taskinfo ti ON t.TaskID = ti.TaskID
                                     WHERE t.Description LIKE @Search 
                                     OR t.Status LIKE @Search 
                                     OR p.ProjectName LIKE @Search";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Search", "%" + searchQuery + "%");
                    List<TaskModel> filteredTasks = new List<TaskModel>();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            filteredTasks.Add(new TaskModel
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
                    TasksDataGrid.ItemsSource = filteredTasks;
                    if (filteredTasks.Count == 0)
                    {
                        MessageBox.Show("Ни одной задачи не найдено по заданному запросу.", "Результаты поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Ошибка при поиске задач: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskResetButton_Click(object sender, RoutedEventArgs e)
        {
            TaskSearchTextBox.Clear();
            LoadTasksData();
        }

        private void TaskSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TaskSearchButton_Click(sender, e);
            }
        }

        #endregion

        #region Управление Учетными Записями

        private void LoadAccounts()
        {
            try
            {
                using (MySqlConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT u.UserID, u.Name, u.Surname, uc.Login, uc.Password 
                                    FROM users u
                                    INNER JOIN usercredentials uc ON u.UserID = uc.UserID
                                    ORDER BY u.UserID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        var accounts = new List<UserAccount>();
                        while (reader.Read())
                        {
                            accounts.Add(new UserAccount
                            {
                                UserID = reader.GetInt32("UserID"),
                                Name = reader.GetString("Name"),
                                Surname = reader.GetString("Surname"),
                                Login = reader.GetString("Login"),
                                Password = reader.GetString("Password")
                            });
                        }
                        AccountsDataGrid.ItemsSource = accounts;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is UserAccount account)
            {
                account.IsPasswordVisible = !account.IsPasswordVisible;
            }
        }

        private void AccountSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = AccountSearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadAccounts();
                return;
            }
            var filtered = ((List<UserAccount>)AccountsDataGrid.ItemsSource)
                .Where(a => a.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            a.Surname.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            a.Login.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
            AccountsDataGrid.ItemsSource = filtered;
            if (filtered.Count == 0)
            {
                MessageBox.Show("Ни одной учетной записи не найдено по заданному запросу.", "Результаты поиска", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AccountResetButton_Click(object sender, RoutedEventArgs e)
        {
            AccountSearchTextBox.Clear();
            LoadAccounts();
        }

        private void AccountSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AccountSearchButton_Click(sender, e);
            }
        }

        private void EditAccountButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = Convert.ToInt32((sender as Button).Tag);
            EditAccountWindow editWindow = new EditAccountWindow(userId);
            editWindow.Owner = this;
            if (editWindow.ShowDialog() == true)
            {
                LoadAccounts();
            }
        }

        public class UserAccount
        {
            public int UserID { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public bool IsPasswordVisible { get; set; }
            public string DisplayPassword => IsPasswordVisible ? Password : new string('•', Password?.Length ?? 0);
        }

        #endregion

        #region Управление Назначением Задач

        public class UserTask
        {
            public string FullName { get; set; }
            public string Description { get; set; }
            public string ProjectName { get; set; }
            public string Status { get; set; }
        }

        public void LoadUserTasks()
        {
            ObservableCollection<UserTask> userTasks = new ObservableCollection<UserTask>();
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT 
                                    CONCAT(u.Name, ' ', u.Surname) AS FullName, 
                                    t.Description, 
                                    p.ProjectName, 
                                    t.Status
                                    FROM user_tasks ut
                                    JOIN users u ON ut.UserID = u.UserID
                                    JOIN tasks t ON ut.TaskID = t.TaskID
                                    JOIN projects p ON ut.ProjectID = p.ProjectID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userTasks.Add(new UserTask
                            {
                                FullName = reader.GetString("FullName"),
                                Description = reader.GetString("Description"),
                                ProjectName = reader.GetString("ProjectName"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                    UserTasksDataGrid.ItemsSource = userTasks;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных задач: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void UserTaskSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = UserTaskSearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadUserTasks();
                return;
            }
            try
            {
                using (MySqlConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT 
                                    CONCAT(u.Name, ' ', u.Surname) AS FullName, 
                                    t.Description, 
                                    p.ProjectName, 
                                    t.Status
                                    FROM user_tasks ut
                                    JOIN users u ON ut.UserID = u.UserID
                                    JOIN tasks t ON ut.TaskID = t.TaskID
                                    JOIN projects p ON ut.ProjectID = p.ProjectID
                                    WHERE u.Name LIKE @Search 
                                    OR u.Surname LIKE @Search 
                                    OR t.Description LIKE @Search 
                                    OR p.ProjectName LIKE @Search 
                                    OR t.Status LIKE @Search";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Search", "%" + searchQuery + "%");
                    ObservableCollection<UserTask> filteredTasks = new ObservableCollection<UserTask>();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            filteredTasks.Add(new UserTask
                            {
                                FullName = reader.GetString("FullName"),
                                Description = reader.GetString("Description"),
                                ProjectName = reader.GetString("ProjectName"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                    UserTasksDataGrid.ItemsSource = filteredTasks;
                    if (filteredTasks.Count == 0)
                    {
                        MessageBox.Show("Ни одной задачи не найдено по заданному запросу.", "Результаты поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Ошибка при поиске задач: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserTaskResetButton_Click(object sender, RoutedEventArgs e)
        {
            UserTaskSearchTextBox.Clear();
            LoadUserTasks();
        }

        private void UserTaskSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UserTaskSearchButton_Click(sender, e);
            }
        }

        private void AddParticipationButton_Click(object sender, RoutedEventArgs e)
        {
            AddParticipationWindow addParticipationWindow = new AddParticipationWindow();
            addParticipationWindow.Owner = this;
            if (addParticipationWindow.ShowDialog() == true)
            {
                LoadUserTasks();
            }
        }

        #endregion

        #region Управление Заявками

        private void LoadRequestsData()
        {
            requestsList = new List<RequestModel>();
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            upa.RequestID,
                            u.Name,
                            u.Surname,
                            u.Email,
                            u.Phone,
                            u.DateOfBirth,
                            p.ProjectName,
                            t.Description AS TaskDescription,
                            upa.Status
                        FROM users_pending_approval upa
                        JOIN users u ON upa.UserID = u.UserID
                        JOIN tasks t ON upa.TaskID = t.TaskID
                        JOIN taskinfo ti ON t.TaskID = ti.TaskID
                        JOIN projects p ON t.ProjectID = p.ProjectID
                        WHERE u.Role = 'Волонтер'
                        ORDER BY upa.CreatedAt DESC";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requestsList.Add(new RequestModel
                            {
                                RequestID = reader.GetInt32("RequestID"),
                                Name = reader.GetString("Name"),
                                Surname = reader.GetString("Surname"),
                                Email = reader.GetString("Email"),
                                Phone = reader.GetString("Phone"),
                                DateOfBirth = reader.GetDateTime("DateOfBirth"),
                                ProjectName = reader.GetString("ProjectName"),
                                TaskDescription = reader.GetString("TaskDescription"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                    RequestsDataGrid.ItemsSource = requestsList;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки заявок: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SearchRequestButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchRequestBox.Text.Trim();
            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadRequestsData();
                return;
            }
            try
            {
                using (MySqlConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            upa.RequestID,
                            u.Name,
                            u.Surname,
                            u.Email,
                            u.Phone,
                            u.DateOfBirth,
                            p.ProjectName,
                            t.Description AS TaskDescription,
                            upa.Status
                        FROM users_pending_approval upa
                        JOIN users u ON upa.UserID = u.UserID
                        JOIN tasks t ON upa.TaskID = t.TaskID
                        JOIN taskinfo ti ON t.TaskID = ti.TaskID
                        JOIN projects p ON t.ProjectID = p.ProjectID
                        WHERE u.Role = 'Волонтер'
                        AND (u.Name LIKE @Search 
                             OR u.Surname LIKE @Search 
                             OR u.Email LIKE @Search 
                             OR p.ProjectName LIKE @Search 
                             OR t.Description LIKE @Search 
                             OR upa.Status LIKE @Search)
                        ORDER BY upa.CreatedAt DESC";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Search", "%" + searchQuery + "%");
                    List<RequestModel> filteredRequests = new List<RequestModel>();
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            filteredRequests.Add(new RequestModel
                            {
                                RequestID = reader.GetInt32("RequestID"),
                                Name = reader.GetString("Name"),
                                Surname = reader.GetString("Surname"),
                                Email = reader.GetString("Email"),
                                Phone = reader.GetString("Phone"),
                                DateOfBirth = reader.GetDateTime("DateOfBirth"),
                                ProjectName = reader.GetString("ProjectName"),
                                TaskDescription = reader.GetString("TaskDescription"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }
                    RequestsDataGrid.ItemsSource = filteredRequests;
                    if (filteredRequests.Count == 0)
                    {
                        MessageBox.Show("Ни одной заявки не найдено по заданному запросу.", "Результаты поиска", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Ошибка при поиске заявок: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetRequestButton_Click(object sender, RoutedEventArgs e)
        {
            SearchRequestBox.Clear();
            LoadRequestsData();
        }

        private void SearchRequestBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchRequestButton_Click(sender, e);
            }
        }

        private void ApproveRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = Convert.ToInt32(button.Tag);
            try
            {
                Database.ApproveRequest(requestId);
                MessageBox.Show("Заявка успешно одобрена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRequestsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при одобрении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RejectRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int requestId = Convert.ToInt32(button.Tag);
            try
            {
                Database.RejectRequest(requestId);
                MessageBox.Show("Заявка успешно отклонена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRequestsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отклонении заявки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

     

        #endregion

        #region Вспомогательные классы и методы

        public class UserCredential
        {
            public int UserID { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
            public string DisplayPassword => string.IsNullOrEmpty(Password) ? "Без пароля" : Password;
            public string DisplayLogin => string.IsNullOrEmpty(Login) ? "Без логина" : Login;
        }

        #endregion
    }
}