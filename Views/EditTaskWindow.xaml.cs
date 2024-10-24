// EditTaskWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models; // Добавлено пространство имён для TaskModel

namespace Volunteers.Views
{
    /// <summary>
    /// Логика взаимодействия для EditTaskWindow.xaml
    /// </summary>
    public partial class EditTaskWindow : Window
    {
        private TaskModel task;
        private List<Project> projectsList;

        /// <summary>
        /// Конструктор окна редактирования задачи.
        /// </summary>
        /// <param name="taskToEdit">Задача, которую нужно отредактировать.</param>
        /// <param name="projects">Список проектов для выбора.</param>
        public EditTaskWindow(TaskModel taskToEdit, List<Project> projects)
        {
            InitializeComponent();
            task = taskToEdit;
            projectsList = projects;
            ProjectComboBox.ItemsSource = projectsList;

            // Заполняем поля текущими данными задачи
            ProjectComboBox.SelectedValue = GetProjectID(task.ProjectName);
            DescriptionTextBox.Text = task.Description;
            SetStatusComboBox(task.Status);
            LocationTextBox.Text = task.Location;
            DatePicker.SelectedDate = task.Date;
        }

        /// <summary>
        /// Получает ProjectID по имени проекта.
        /// </summary>
        private int GetProjectID(string projectName)
        {
            Project selectedProject = projectsList.Find(p => p.ProjectName == projectName);
            return selectedProject != null ? selectedProject.ProjectID : 0;
        }

        /// <summary>
        /// Устанавливает выбранный элемент ComboBox для статуса задачи.
        /// </summary>
        private void SetStatusComboBox(string status)
        {
            foreach (var item in StatusComboBox.Items)
            {
                ComboBoxItem comboBoxItem = item as ComboBoxItem;
                if (comboBoxItem != null && comboBoxItem.Content.ToString() == status)
                {
                    StatusComboBox.SelectedItem = comboBoxItem;
                    break;
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить". Обновляет задачу в базе данных.
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация полей
            if (ProjectComboBox.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите проект.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите описание задачи.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите статус задачи.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(LocationTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите местоположение задачи.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату выполнения задачи.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем значения из полей
            int projectId = (int)ProjectComboBox.SelectedValue;
            string description = DescriptionTextBox.Text.Trim();
            string status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString();
            string location = LocationTextBox.Text.Trim();
            DateTime date = DatePicker.SelectedDate.Value;

            // Обновление в базе данных
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // Обновление таблицы tasks
                        string updateTaskQuery = @"UPDATE tasks 
                                                   SET ProjectID = @ProjectID, Description = @Description, Status = @Status 
                                                   WHERE TaskID = @TaskID";
                        MySqlCommand cmdUpdateTask = new MySqlCommand(updateTaskQuery, connection, transaction);
                        cmdUpdateTask.Parameters.AddWithValue("@ProjectID", projectId);
                        cmdUpdateTask.Parameters.AddWithValue("@Description", description);
                        cmdUpdateTask.Parameters.AddWithValue("@Status", status);
                        cmdUpdateTask.Parameters.AddWithValue("@TaskID", task.TaskID);
                        cmdUpdateTask.ExecuteNonQuery();

                        // Обновление таблицы taskinfo
                        string updateTaskInfoQuery = @"UPDATE taskinfo 
                                                       SET Location = @Location, Date = @Date 
                                                       WHERE TaskID = @TaskID";
                        MySqlCommand cmdUpdateTaskInfo = new MySqlCommand(updateTaskInfoQuery, connection, transaction);
                        cmdUpdateTaskInfo.Parameters.AddWithValue("@Location", location);
                        cmdUpdateTaskInfo.Parameters.AddWithValue("@Date", date);
                        cmdUpdateTaskInfo.Parameters.AddWithValue("@TaskID", task.TaskID);
                        cmdUpdateTaskInfo.ExecuteNonQuery();

                        transaction.Commit();

                        MessageBox.Show("Задача успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        // Откат транзакции в случае ошибки
                        transaction.Rollback();
                        MessageBox.Show("Ошибка при обновлении задачи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки "Отмена". Закрывает окно без сохранения.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
