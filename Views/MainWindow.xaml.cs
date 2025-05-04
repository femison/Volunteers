using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;
using BCrypt.Net;
using Volunteers.Views;
using System.Windows.Controls;

namespace Volunteers
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private bool isPasswordVisible = false;

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                VisiblePasswordBox.Text = PasswordBox.Password;
                VisiblePasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = "Скрыть пароль";
            }
            else
            {
                PasswordBox.Password = VisiblePasswordBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = "Показать пароль";
            }
        }

        // Обработчик для кнопки закрытия
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
                LoginTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите пароль.", "Пустое поле", MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            if (AuthenticateUser(login, password, out User authenticatedUser))
            {
                if (authenticatedUser.Role.Equals("Администратор", StringComparison.OrdinalIgnoreCase))
                {
                    AdminWindow adminWindow = new AdminWindow(authenticatedUser);
                    adminWindow.Owner = this;
                    adminWindow.Show();
                    this.Hide();
                }
                else if (authenticatedUser.Role.Equals("Волонтер", StringComparison.OrdinalIgnoreCase))
                {
                    VoliWindow voliWindow = new VoliWindow(authenticatedUser);
                    voliWindow.Owner = this;
                    voliWindow.Show();
                    this.Hide();
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
            VisiblePasswordBox.Clear();
        }
    }
}