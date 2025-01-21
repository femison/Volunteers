// MainWindow.xaml.cs
using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;
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
                                        u.DateOfBirth, u.Gender, u.Address, u.Role
                                 FROM usercredentials uc
                                 JOIN users u ON uc.UserID = u.UserID
                                 WHERE uc.Login = @Login AND uc.Password = @Password";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", login);
                    cmd.Parameters.AddWithValue("@Password", password); // Пароль передается напрямую

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
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

                    return false;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        // Обработчик закрытия окна
        private void Window_Closed(object sender, EventArgs e)
        {
            Logout();
        }

        // Метод выхода из системы
        private void Logout()
        {
            if (this.Owner != null)
            {
                MainWindow mainWindow = this.Owner as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.ClearLoginFields(); // Очищаем поля логина и пароля
                    mainWindow.Show();              // Показываем MainWindow
                }
            }
            this.Close(); // Закрываем текущее окно (AdminWindow или VoliWindow)
        }

        // Метод для очистки полей ввода
        public void ClearLoginFields()
        {
            LoginTextBox.Clear();
            PasswordBox.Clear();
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Переносим пароль из PasswordBox в TextBox
            ShowPasswordTextBox.Text = PasswordBox.Password;
            // Показываем TextBox и скрываем PasswordBox
            ShowPasswordTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;

            // Устанавливаем фокус на TextBox
            ShowPasswordTextBox.Focus();
            ShowPasswordTextBox.CaretIndex = ShowPasswordTextBox.Text.Length;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Переносим пароль из TextBox обратно в PasswordBox
            PasswordBox.Password = ShowPasswordTextBox.Text;
            // Показываем PasswordBox и скрываем TextBox
            PasswordBox.Visibility = Visibility.Visible;
            ShowPasswordTextBox.Visibility = Visibility.Collapsed;

            // Устанавливаем фокус на PasswordBox
            PasswordBox.Focus();
        }
    }
}
