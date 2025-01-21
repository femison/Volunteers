// Views/AdminWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Для обработки клавиш
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
        private List<TaskModel> tasksList; // Список задач

        public AdminWindow(User user)
{
            InitializeComponent();
            currentUser = user;
            LoadUsersData();
            LoadProjectsData();
            LoadTasksData();
            LoadAccounts();
            LoadUserTasks(); // Добавьте вызов загрузки назначений
}


        #region Управление Пользователями

        /// <summary>
        /// Загружает данные пользователей из базы данных и отображает их в DataGrid.
        /// </summary>
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
                            User user = new User
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
                            };
                            usersList.Add(user);
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


        /// <summary>
        /// Открывает окно для добавления нового пользователя.
        /// </summary>
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            AddUserWindow addUserWindow = new AddUserWindow();
            addUserWindow.Owner = this;

            if (addUserWindow.ShowDialog() == true)
            {
                LoadUsersData();
            }
        }

        /// <summary>
        /// Открывает окно для редактирования выбранного пользователя.
        /// </summary>
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

        /// <summary>
        /// Удаляет выбранного пользователя из базы данных после подтверждения.
        /// </summary>
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            int userId = Convert.ToInt32((sender as Button).Tag);

            // Подтверждение удаления
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

        /// <summary>
        /// Удаляет пользователя из базы данных вместе с его учетными данными.
        /// </summary>
        private bool DeleteUserFromDatabase(int userId)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    // Удаление учетных данных
                    string deleteCredentialQuery = "DELETE FROM usercredentials WHERE UserID=@UserID";
                    MySqlCommand cmdCredential = new MySqlCommand(deleteCredentialQuery, connection, transaction);
                    cmdCredential.Parameters.AddWithValue("@UserID", userId);
                    cmdCredential.ExecuteNonQuery();

                    // Удаление пользователя
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

        /// <summary>
        /// Обработчик нажатия кнопки "Поиск" для пользователей.
        /// Выполняет поиск пользователей по имени, фамилии, email и роли.
        /// </summary>
        private void UserSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = UserSearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                // Если поле поиска пустое, загрузить всех пользователей
                LoadUsersData();
                return;
            }

            // Выполнить поиск пользователей по имени, фамилии, email или роли
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
                            User user = new User
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
                            };
                            filteredUsers.Add(user);
                        }
                    }

                    UsersDataGrid.ItemsSource = filteredUsers;

                    // Если не найдено ни одного пользователя, уведомить пользователя
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

        /// <summary>
        /// Обработчик нажатия кнопки "Сбросить" для пользователей.
        /// Очищает поле поиска и загружает всех пользователей.
        /// </summary>
        private void UserResetButton_Click(object sender, RoutedEventArgs e)
        {
            UserSearchTextBox.Clear(); // Очистить поле поиска
            LoadUsersData();           // Загрузить всех пользователей
        }

        /// <summary>
        /// Обработчик нажатия клавиши Enter в поле поиска пользователей.
        /// Позволяет запускать поиск по нажатию Enter.
        /// </summary>
        private void UserSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UserSearchButton_Click(sender, e);
            }
        }

        #endregion

        #region Управление Проектами

        /// <summary>
        /// Загружает данные проектов из базы данных и отображает их в DataGrid.
        /// </summary>
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
                            Project project = new Project
                            {
                                ProjectID = reader.GetInt32("ProjectID"),
                                ProjectName = reader.GetString("ProjectName"),
                                StartDate = reader.GetDateTime("StartDate"),
                                EndDate = reader.GetDateTime("EndDate"),
                                Status = reader.GetString("Status")
                            };
                            projectsList.Add(project);
                        }
                    }

                    ProjectsDataGrid.ItemsSource = projectsList;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки данных проектов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // После загрузки проектов, обновляем задачи
            LoadTasksData();
        }

        /// <summary>
        /// Открывает окно для добавления нового проекта.
        /// </summary>
        private void AddProjectButton_Click(object sender, RoutedEventArgs e)
        {
            AddProjectWindow addProjectWindow = new AddProjectWindow();
            addProjectWindow.Owner = this;

            if (addProjectWindow.ShowDialog() == true)
            {
                LoadProjectsData();
            }
        }

        /// <summary>
        /// Открывает окно для редактирования выбранного проекта.
        /// </summary>
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

        /// <summary>
        /// Удаляет выбранный проект из базы данных после подтверждения.
        /// </summary>
        private void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
        {
            int projectId = Convert.ToInt32((sender as Button).Tag);

            // Подтверждение удаления
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

        /// <summary>
        /// Удаляет проект из базы данных вместе со связанными задачами.
        /// </summary>
        private bool DeleteProjectFromDatabase(int projectId)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    // Удаление задач, связанных с проектом
                    string deleteTasksQuery = "DELETE FROM tasks WHERE ProjectID=@ProjectID";
                    MySqlCommand cmdTasks = new MySqlCommand(deleteTasksQuery, connection, transaction);
                    cmdTasks.Parameters.AddWithValue("@ProjectID", projectId);
                    cmdTasks.ExecuteNonQuery();

                    // Удаление проектов
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

        /// <summary>
        /// Обработчик нажатия кнопки "Поиск" для проектов.
        /// Выполняет поиск проектов по названию или статусу.
        /// </summary>
        private void ProjectSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = ProjectSearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                // Если поле поиска пустое, загрузить все проекты
                LoadProjectsData();
                return;
            }

            // Выполнить поиск проектов по названию или статусу
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
                            Project project = new Project
                            {
                                ProjectID = reader.GetInt32("ProjectID"),
                                ProjectName = reader.GetString("ProjectName"),
                                StartDate = reader.GetDateTime("StartDate"),
                                EndDate = reader.GetDateTime("EndDate"),
                                Status = reader.GetString("Status")
                            };
                            filteredProjects.Add(project);
                        }
                    }

                    ProjectsDataGrid.ItemsSource = filteredProjects;

                    // Если не найдено ни одного проекта, уведомить пользователя
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

        /// <summary>
        /// Обработчик нажатия кнопки "Сбросить" для проектов.
        /// Очищает поле поиска и загружает все проекты.
        /// </summary>
        private void ProjectResetButton_Click(object sender, RoutedEventArgs e)
        {
            ProjectSearchTextBox.Clear(); // Очистить поле поиска
            LoadProjectsData();           // Загрузить все проекты
        }

        /// <summary>
        /// Обработчик нажатия клавиши Enter в поле поиска проектов.
        /// Позволяет запускать поиск по нажатию Enter.
        /// </summary>
        private void ProjectSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProjectSearchButton_Click(sender, e);
            }
        }

        #endregion

        #region Управление Задачами

        /// <summary>
        /// Загружает данные задач из базы данных и отображает их в DataGrid.
        /// </summary>
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
                            TaskModel task = new TaskModel
                            {
                                TaskID = reader.GetInt32("TaskID"),
                                ProjectName = reader.GetString("ProjectName"),
                                Description = reader.GetString("Description"),
                                Status = reader.GetString("Status"),
                                Location = reader.GetString("Location"),
                                Date = reader.GetDateTime("Date")
                            };
                            tasksList.Add(task);
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

        /// <summary>
        /// Открывает окно для добавления новой задачи.
        /// </summary>
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            AddTaskWindow addTaskWindow = new AddTaskWindow(projectsList); // Передаем список проектов для выбора
            addTaskWindow.Owner = this;

            if (addTaskWindow.ShowDialog() == true)
            {
                LoadTasksData();
            }
        }

        /// <summary>
        /// Открывает окно для редактирования выбранной задачи.
        /// </summary>
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

        /// <summary>
        /// Удаляет выбранную задачу из базы данных после подтверждения.
        /// </summary>
        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            int taskId = Convert.ToInt32((sender as Button).Tag);

            // Подтверждение удаления
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

        /// <summary>
        /// Удаляет задачу из базы данных вместе с информацией о задаче.
        /// </summary>
        private bool DeleteTaskFromDatabase(int taskId)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    // Удаление из таблицы user_tasks
                    string deleteUserTasksQuery = "DELETE FROM user_tasks WHERE TaskID=@TaskID";
                    MySqlCommand cmdUserTasks = new MySqlCommand(deleteUserTasksQuery, connection, transaction);
                    cmdUserTasks.Parameters.AddWithValue("@TaskID", taskId);
                    cmdUserTasks.ExecuteNonQuery();

                    // Удаление из taskinfo
                    string deleteTaskInfoQuery = "DELETE FROM taskinfo WHERE TaskID=@TaskID";
                    MySqlCommand cmdTaskInfo = new MySqlCommand(deleteTaskInfoQuery, connection, transaction);
                    cmdTaskInfo.Parameters.AddWithValue("@TaskID", taskId);
                    cmdTaskInfo.ExecuteNonQuery();

                    // Удаление из tasks
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

        /// <summary>
        /// Обработчик нажатия кнопки "Поиск" для задач.
        /// Выполняет поиск задач по описанию, статусу или проекту.
        /// </summary>
        private void TaskSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = TaskSearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                // Если поле поиска пустое, загрузить все задачи
                LoadTasksData();
                return;
            }

            // Выполнить поиск задач по описанию, статусу или названию проекта
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
                            TaskModel task = new TaskModel
                            {
                                TaskID = reader.GetInt32("TaskID"),
                                ProjectName = reader.GetString("ProjectName"),
                                Description = reader.GetString("Description"),
                                Status = reader.GetString("Status"),
                                Location = reader.GetString("Location"),
                                Date = reader.GetDateTime("Date")
                            };
                            filteredTasks.Add(task);
                        }
                    }

                    TasksDataGrid.ItemsSource = filteredTasks;

                    // Если не найдено ни одной задачи, уведомить пользователя
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

        /// <summary>
        /// Обработчик нажатия кнопки "Сбросить" для задач.
        /// Очищает поле поиска и загружает все задачи.
        /// </summary>
        private void TaskResetButton_Click(object sender, RoutedEventArgs e)
        {
            TaskSearchTextBox.Clear(); // Очистить поле поиска
            LoadTasksData();           // Загрузить все задачи
        }

        /// <summary>
        /// Обработчик нажатия клавиши Enter в поле поиска задач.
        /// Позволяет запускать поиск по нажатию Enter.
        /// </summary>
        private void TaskSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TaskSearchButton_Click(sender, e);
            }
        }

        #endregion

        /// <summary>
        /// Обработчик закрытия окна администратора.
        /// Возвращает пользователя к окну авторизации.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.Owner != null)
            {
                MainWindow mainWindow = this.Owner as MainWindow;
                if (mainWindow != null)
                {
                    // Очищаем поля логина и пароля
                    mainWindow.ClearLoginFields();
                    mainWindow.Show(); // Показываем окно авторизации
                }
            }
        }
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
                    MySqlDataReader reader = cmd.ExecuteReader();

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
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is UserAccount account)
            {
                account.IsPasswordVisible = !account.IsPasswordVisible;
            }
        }

        // В AdminWindow.xaml.cs
        private void AccountSearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = AccountSearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadAccounts();
                return;
            }

            var filtered = ((List<UserAccount>)AccountsDataGrid.ItemsSource)
                .Where(a => a.Name.Contains(searchQuery) ||
                           a.Surname.Contains(searchQuery) ||
                           a.Login.Contains(searchQuery))
                .ToList();

            AccountsDataGrid.ItemsSource = filtered;
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
        private List<UserTaskAssignment> _allUserTasks = new List<UserTaskAssignment>();
        // Загрузка данных
        private void LoadUserTasks()
        {
            try
            {
                using (MySqlConnection connection = Database.GetConnection())
                {
                    connection.Open();
                    string query = @"SELECT 
                                ut.UserTaskID, 
                                CONCAT(u.Name, ' ', u.Surname) AS FullName, 
                                t.Description, 
                                p.ProjectName 
                            FROM user_tasks ut
                            JOIN users u ON ut.UserID = u.UserID
                            JOIN tasks t ON ut.TaskID = t.TaskID
                            JOIN projects p ON ut.ProjectID = p.ProjectID";

                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    _allUserTasks.Clear();
                    while (reader.Read())
                    {
                        _allUserTasks.Add(new UserTaskAssignment
                        {
                            UserTaskID = reader.GetInt32("UserTaskID"),
                            FullName = reader.GetString("FullName"),
                            Description = reader.GetString("Description"),
                            ProjectName = reader.GetString("ProjectName")
                        });
                    }

                    UserTasksDataGrid.ItemsSource = _allUserTasks;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки назначений: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserTaskSearchButton_Click(object sender, RoutedEventArgs e)
        {
            ApplyUserTaskFilter();
        }

        private void UserTaskResetButton_Click(object sender, RoutedEventArgs e)
        {
            UserTaskSearchTextBox.Clear();
            UserTasksDataGrid.ItemsSource = _allUserTasks;
        }

        private void UserTaskSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyUserTaskFilter();
            }
        }

        private void ApplyUserTaskFilter()
        {
            string searchText = UserTaskSearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                UserTasksDataGrid.ItemsSource = _allUserTasks;
                return;
            }

            var filtered = _allUserTasks
                .Where(task =>
                    (task.FullName?.ToLower().Contains(searchText) ?? false) ||
                    (task.Description?.ToLower().Contains(searchText) ?? false) ||
                    (task.ProjectName?.ToLower().Contains(searchText) ?? false))
                .ToList();

            UserTasksDataGrid.ItemsSource = filtered;
        }


        private void AddUserTaskButton_Click(object sender, RoutedEventArgs e)
        {
            //Добавление задачи пользователю
        }
        private void RefreshUserTasksButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserTasks();
        }

    }
    

    }
