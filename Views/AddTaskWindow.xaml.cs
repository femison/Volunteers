// AddTaskWindow.xaml.cs
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
    /// Логика взаимодействия для AddTaskWindow.xaml
    /// </summary>
    public partial class AddTaskWindow : Window
    {
        private List<Project> projectsList;

        /// <summary>
        /// Конструктор окна добавления задачи.
        /// </summary>
        /// <param name="projects">Список проектов для выбора.</param>
        public AddTaskWindow(List<Project> projects)
        {
            InitializeComponent();
            projectsList = projects;
            ProjectComboBox.ItemsSource = projectsList;
            if (projectsList.Count > 0)
            {
                ProjectComboBox.SelectedIndex = 0; // Устанавливаем первый проект по умолчанию
            }

            // Устанавливаем текущую дату по умолчанию
            DatePicker.SelectedDate = DateTime.Today;
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить". Добавляет задачу в базу данных.
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

            // Вставка в базу данных
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // Вставка в таблицу tasks
                        string insertTaskQuery = @"INSERT INTO tasks (ProjectID, Description, Status) 
                                                   VALUES (@ProjectID, @Description, @Status)";
                        MySqlCommand cmdInsertTask = new MySqlCommand(insertTaskQuery, connection, transaction);
                        cmdInsertTask.Parameters.AddWithValue("@ProjectID", projectId);
                        cmdInsertTask.Parameters.AddWithValue("@Description", description);
                        cmdInsertTask.Parameters.AddWithValue("@Status", status);
                        cmdInsertTask.ExecuteNonQuery();

                        // Получение TaskID только что вставленной записи
                        long taskId = cmdInsertTask.LastInsertedId;

                        // Вставка в таблицу taskinfo
                        string insertTaskInfoQuery = @"INSERT INTO taskinfo (TaskID, Location, Date) 
                                                       VALUES (@TaskID, @Location, @Date)";
                        MySqlCommand cmdInsertTaskInfo = new MySqlCommand(insertTaskInfoQuery, connection, transaction);
                        cmdInsertTaskInfo.Parameters.AddWithValue("@TaskID", taskId);
                        cmdInsertTaskInfo.Parameters.AddWithValue("@Location", location);
                        cmdInsertTaskInfo.Parameters.AddWithValue("@Date", date);
                        cmdInsertTaskInfo.ExecuteNonQuery();

                        // Подключение задачи к проекту (если необходимо)
                        // Здесь можно добавить дополнительную логику, например, уведомление пользователей или обновление статуса проекта

                        // Коммит транзакции
                        transaction.Commit();

                        MessageBox.Show("Задача успешно добавлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        // Откат транзакции в случае ошибки
                        transaction.Rollback();
                        MessageBox.Show("Ошибка при добавлении задачи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
