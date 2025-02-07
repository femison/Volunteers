using MySql.Data.MySqlClient;
using System;
using System.Windows;
using Volunteers.Data;
using Volunteers.Models; // Если нужно для связи с моделями

namespace Volunteers.Views
{
    public partial class AddParticipationWindow : Window
    {
        public AddParticipationWindow()
        {
            InitializeComponent();
            LoadProjects();  // Загружаем проекты
            LoadUsers();     // Загружаем пользователей
        }


        private void LoadProjects()
        {
            var projects = GetProjects(); // Загружаем проекты из базы данных
            ProjectComboBox.ItemsSource = projects;
            ProjectComboBox.DisplayMemberPath = "ProjectName";  // Отображать название проекта
            ProjectComboBox.SelectedValuePath = "ProjectID";  // Выбирать по ID проекта
        }

        private void LoadTasks(int projectId)
        {
            var tasks = GetTasksForProject(projectId); // Получаем задачи для выбранного проекта
            TaskComboBox.ItemsSource = tasks;
            TaskComboBox.DisplayMemberPath = "Description"; // Отображать описание задачи
            TaskComboBox.SelectedValuePath = "TaskID"; // Выбирать задачу по её ID
        }


        private void LoadUsers()
        {
            var users = GetUsers();  // Получаем список пользователей
            UserComboBox.ItemsSource = users;
            UserComboBox.DisplayMemberPath = "FullName";  // Для отображения ФИО
            UserComboBox.SelectedValuePath = "UserID";  // ID для выбора
        }

        private List<User> GetUsers()
        {
            var users = new List<User>();

            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT UserID, Name, Surname FROM users";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                UserID = reader.GetInt32("UserID"),
                                Name = reader.GetString("Name"),
                                Surname = reader.GetString("Surname")
                            };
                            users.Add(user);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки пользователей: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return users;
        }


        private void SaveParticipationButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedProject = (Project)ProjectComboBox.SelectedItem;
            var selectedTask = (TaskModel)TaskComboBox.SelectedItem;
            var selectedUser = (User)UserComboBox.SelectedItem;

            if (selectedProject != null && selectedTask != null && selectedUser != null)
            {
                AddUserToTask(selectedUser.UserID, selectedTask.TaskID, selectedProject.ProjectID);
                MessageBox.Show("Участие успешно добавлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Уведомление об успешном добавлении и обновление данных на главной форме
                if (this.Owner is AdminWindow adminWindow)
                {
                    adminWindow.LoadUserTasks(); // Обновление данных на главной форме
                }

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите проект, задачу и пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }




        private void AddUserToTask(int userId, int taskId, int projectId)
        {
            using (var connection = Database.GetConnection())
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



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ProjectComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ProjectComboBox.SelectedItem is Project selectedProject)
            {
                LoadTasks(selectedProject.ProjectID);
            }
        }

        // Модели данных
        private void AddUserToTask(int projectId, int taskId)
        {
            // Добавление волонтера в задачу
            // Реализуйте добавление в базу данных или в список
        }

        private List<Project> GetProjects()
        {
            var projects = new List<Project>();

            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ProjectID, ProjectName FROM projects";
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Project project = new Project
                            {
                                ProjectID = reader.GetInt32("ProjectID"),
                                ProjectName = reader.GetString("ProjectName")
                            };
                            projects.Add(project);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки проектов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return projects;
        }


        private List<TaskModel> GetTasksForProject(int projectId)
        {
            var tasks = new List<TaskModel>();

            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT t.TaskID, t.Description
                             FROM tasks t
                             WHERE t.ProjectID = @ProjectID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ProjectID", projectId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TaskModel task = new TaskModel
                            {
                                TaskID = reader.GetInt32("TaskID"),
                                Description = reader.GetString("Description")
                            };
                            tasks.Add(task);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка загрузки задач: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return tasks;
        }

        private void CancelParticipationButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }


       

    }
}
