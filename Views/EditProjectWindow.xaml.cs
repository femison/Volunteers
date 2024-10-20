// EditProjectWindow.xaml.cs
using System;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;

namespace Volunteers.Views
{
    public partial class EditProjectWindow : Window
    {
        private Project projectToEdit;

        public EditProjectWindow(Project project)
        {
            InitializeComponent();
            projectToEdit = project;
            PopulateFields();
        }

        private void PopulateFields()
        {
            ProjectNameTextBox.Text = projectToEdit.ProjectName;
            StartDatePicker.SelectedDate = projectToEdit.StartDate;
            EndDatePicker.SelectedDate = projectToEdit.EndDate;
            StatusComboBox.SelectedItem = GetComboBoxItemByContent(StatusComboBox, projectToEdit.Status);
        }

        private ComboBoxItem GetComboBoxItemByContent(ComboBox comboBox, string content)
        {
            foreach (var item in comboBox.Items)
            {
                if ((item as ComboBoxItem)?.Content.ToString() == content)
                {
                    return item as ComboBoxItem;
                }
            }
            return null;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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

            // Обновление объекта проекта
            projectToEdit.ProjectName = ProjectNameTextBox.Text.Trim();
            projectToEdit.StartDate = StartDatePicker.SelectedDate.Value;
            projectToEdit.EndDate = EndDatePicker.SelectedDate.Value;
            projectToEdit.Status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Обновление проекта в базе данных
            if (UpdateProjectInDatabase(projectToEdit))
            {
                MessageBox.Show("Проект успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool UpdateProjectInDatabase(Project project)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE projects 
                                     SET ProjectName=@ProjectName, StartDate=@StartDate, EndDate=@EndDate, Status=@Status
                                     WHERE ProjectID=@ProjectID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@ProjectName", project.ProjectName);
                    cmd.Parameters.AddWithValue("@StartDate", project.StartDate);
                    cmd.Parameters.AddWithValue("@EndDate", project.EndDate);
                    cmd.Parameters.AddWithValue("@Status", project.Status);
                    cmd.Parameters.AddWithValue("@ProjectID", project.ProjectID);

                    int result = cmd.ExecuteNonQuery();
                    return result > 0;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при обновлении проекта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
