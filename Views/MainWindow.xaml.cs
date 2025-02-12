using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;
using BCrypt.Net;
using Volunteers.Views;

namespace Volunteers
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Обработчик нажатия кнопки "Войти"
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Пожалуйста, введите логин.", "Пустое поле", MessageBoxButton.OK, MessageBoxImage.Warning);
                LoginTextBox.Focus(); // Устанавливаем фокус на поле логина
                return; // Прерываем дальнейшее выполнение метода
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите пароль.", "Пустое поле", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus(); // Устанавливаем фокус на поле пароля
                return; // Прерываем дальнейшее выполнение метода
            }

            if (AuthenticateUser(login, password, out User authenticatedUser))
            {
                if (authenticatedUser.Role.Equals("Администратор", StringComparison.OrdinalIgnoreCase))
                {
                    AdminWindow adminWindow = new AdminWindow(authenticatedUser);
                    adminWindow.Owner = this; // Устанавливаем владельца окна
                    adminWindow.Show();
                    this.Hide(); // Скрываем MainWindow
                }
                else if (authenticatedUser.Role.Equals("Волонтер", StringComparison.OrdinalIgnoreCase))
                {
                    // Открываем окно VoliWindow для роли "Волонтёр"
                    VoliWindow voliWindow = new VoliWindow(authenticatedUser);
                    voliWindow.Owner = this; // Устанавливаем владельца окна
                    voliWindow.Show();
                    this.Hide(); // Скрываем MainWindow
                }
                else
                {
                    MessageBox.Show("Неизвестная роль пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Метод аутентификации пользователя
        private bool AuthenticateUser(string login, string password, out User user)
        {
            user = null;

            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT u.UserID, u.Name, u.Surname, u.UserSkills, u.Email, u.Phone, 
                                        u.DateOfBirth, u.Gender, u.Address, u.Role, uc.Password
                                 FROM usercredentials uc
                                 JOIN users u ON uc.UserID = u.UserID
                                 WHERE uc.Login = @Login";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", login);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPasswordHash = reader.GetString("Password");

                            // Проверка пароля с хэшированным значением
                            if (BCrypt.Net.BCrypt.Verify(password, storedPasswordHash))
                            {
                                user = new User
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
                                return true;
                            }
                        }
                    }

                    return false;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }
        public void ClearLoginFields()
        {
            LoginTextBox.Clear();
            PasswordBox.Clear();
        }
    }
}
