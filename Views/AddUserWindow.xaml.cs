// AddUserWindow.xaml.cs
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;

namespace Volunteers.Views
{
    public partial class AddUserWindow : Window
    {
        public AddUserWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(SurnameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                DateOfBirthPicker.SelectedDate == null ||
                GenderComboBox.SelectedItem == null ||
                RoleComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Хэширование пароля
            

            // Создание объекта пользователя
            User newUser = new User
            {
                Name = NameTextBox.Text.Trim(),
                Surname = SurnameTextBox.Text.Trim(),
                UserSkills = UserSkillsTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                Phone = PhoneTextBox.Text.Trim(),
                DateOfBirth = DateOfBirthPicker.SelectedDate.Value,
                Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Address = AddressTextBox.Text.Trim(),
                Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
            };

            // Создание объекта учетных данных
            UserCredential credentials = new UserCredential
            {
                Login = LoginTextBox.Text.Trim(),
                Password = PasswordBox.Password.Trim()
            };

            // Добавление пользователя в базу данных
            if (AddUserToDatabase(newUser, credentials))
            {
                MessageBox.Show("Пользователь успешно добавлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool AddUserToDatabase(User user, UserCredential credentials)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    // Вставка пользователя
                    string insertUserQuery = @"INSERT INTO users (Name, Surname, UserSkills, Email, Phone, DateOfBirth, Gender, Address, Role)
                                               VALUES (@Name, @Surname, @UserSkills, @Email, @Phone, @DateOfBirth, @Gender, @Address, @Role)";
                    MySqlCommand cmdUser = new MySqlCommand(insertUserQuery, connection, transaction);
                    cmdUser.Parameters.AddWithValue("@Name", user.Name);
                    cmdUser.Parameters.AddWithValue("@Surname", user.Surname);
                    cmdUser.Parameters.AddWithValue("@UserSkills", user.UserSkills);
                    cmdUser.Parameters.AddWithValue("@Email", user.Email);
                    cmdUser.Parameters.AddWithValue("@Phone", user.Phone);
                    cmdUser.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
                    cmdUser.Parameters.AddWithValue("@Gender", user.Gender);
                    cmdUser.Parameters.AddWithValue("@Address", user.Address);
                    cmdUser.Parameters.AddWithValue("@Role", user.Role);

                    cmdUser.ExecuteNonQuery();
                    int insertedUserId = (int)cmdUser.LastInsertedId;

                    // Вставка учетных данных
                    string insertCredentialQuery = @"INSERT INTO usercredentials (UserID, Login, Password)
                                                    VALUES (@UserID, @Login, @Password)";
                    MySqlCommand cmdCredential = new MySqlCommand(insertCredentialQuery, connection, transaction);
                    cmdCredential.Parameters.AddWithValue("@UserID", insertedUserId);
                    cmdCredential.Parameters.AddWithValue("@Login", credentials.Login);
                    cmdCredential.Parameters.AddWithValue("@Password", credentials.Password);

                    cmdCredential.ExecuteNonQuery();

                    // Коммит транзакции
                    transaction.Commit();
                    return true;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при добавлении пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
