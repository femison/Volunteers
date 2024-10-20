// AddProjectWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;

namespace Volunteers.Views
{
    public partial class AddProjectWindow : Window
    {
        public AddProjectWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text) ||
                StartDatePicker.SelectedDate == null ||
                EndDatePicker.SelectedDate == null ||
                StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка, что дата окончания не раньше даты начала
            if (EndDatePicker.SelectedDate < StartDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создание объекта проекта
            Project newProject = new Project
            {
                ProjectName = ProjectNameTextBox.Text.Trim(),
                StartDate = StartDatePicker.SelectedDate.Value,
                EndDate = EndDatePicker.SelectedDate.Value,
                Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
            };

            // Добавление проекта в базу данных
            if (AddProjectToDatabase(newProject))
            {
                MessageBox.Show("Проект успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AddProjectToDatabase(Project project)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"INSERT INTO projects (ProjectName, StartDate, EndDate, Status)
                                     VALUES (@ProjectName, @StartDate, @EndDate, @Status)";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                    cmd.Parameters.AddWithValue("@StartDate", project.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", project.EndDate);
                    cmd.Parameters.AddWithValue("@Status", project.Status);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при добавлении проекта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
