using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Volunteers.Data;
using Volunteers.Models;
using BCrypt.Net;
using System.Windows.Controls;  // Подключаем BCrypt

namespace Volunteers.Views
{
    public partial class EditUserWindow : Window
    {
        private User userToEdit;

        // Конструктор, принимающий объект User
        public EditUserWindow(User user)
        {
            InitializeComponent();
            userToEdit = user;
            PopulateFields();
        }

        private void PopulateFields()
        {
            // Заполнение полей окна данными пользователя
            NameTextBox.Text = userToEdit.Name;
            SurnameTextBox.Text = userToEdit.Surname;
            UserSkillsTextBox.Text = userToEdit.UserSkills;
            EmailTextBox.Text = userToEdit.Email;
            PhoneTextBox.Text = userToEdit.Phone;
            DateOfBirthPicker.SelectedDate = userToEdit.DateOfBirth;
            GenderComboBox.SelectedItem = GetComboBoxItemByContent(GenderComboBox, userToEdit.Gender);
            AddressTextBox.Text = userToEdit.Address;
            RoleComboBox.SelectedItem = GetComboBoxItemByContent(RoleComboBox, userToEdit.Role);
            LoginTextBox.Text = GetUserLogin(userToEdit.UserID);
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

        private string GetUserLogin(int userId)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Login FROM usercredentials WHERE UserID=@UserID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "";
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при получении логина пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
                }
            }
        }

        // Добавьте проверку и обновление пароля, если он был изменен

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(SurnameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                DateOfBirthPicker.SelectedDate == null ||
                GenderComboBox.SelectedItem == null ||
                RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Обновление объекта пользователя
            userToEdit.Name = NameTextBox.Text.Trim();
            userToEdit.Surname = SurnameTextBox.Text.Trim();
            userToEdit.UserSkills = UserSkillsTextBox.Text.Trim();
            userToEdit.Email = EmailTextBox.Text.Trim();
            userToEdit.Phone = PhoneTextBox.Text.Trim();
            userToEdit.DateOfBirth = DateOfBirthPicker.SelectedDate.Value;
            userToEdit.Gender = (GenderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            userToEdit.Address = AddressTextBox.Text.Trim();
            userToEdit.Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Проверка на изменение пароля
            string newPasswordHash = "";
            if (!string.IsNullOrWhiteSpace(NewPasswordBox.Password))
            {
                newPasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPasswordBox.Password.Trim());  // Хэшируем новый пароль с помощью BCrypt
            }
            else
            {
                newPasswordHash = GetUserPassword(userToEdit.UserID); // Оставляем текущий пароль
            }

            // Обновление пользователя в базе данных
            if (UpdateUserInDatabase(userToEdit, newPasswordHash))
            {
                MessageBox.Show("Пользователь успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool UpdateUserInDatabase(User user, string passwordHash)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    MySqlTransaction transaction = connection.BeginTransaction();

                    // Обновление данных пользователя
                    string updateUserQuery = @"UPDATE users 
                                       SET Name=@Name, Surname=@Surname, UserSkills=@UserSkills, Email=@Email, Phone=@Phone, 
                                           DateOfBirth=@DateOfBirth, Gender=@Gender, Address=@Address, Role=@Role
                                       WHERE UserID=@UserID";
                    MySqlCommand cmdUser = new MySqlCommand(updateUserQuery, connection, transaction);
                    cmdUser.Parameters.AddWithValue("@Name", user.Name);
                    cmdUser.Parameters.AddWithValue("@Surname", user.Surname);
                    cmdUser.Parameters.AddWithValue("@UserSkills", user.UserSkills);
                    cmdUser.Parameters.AddWithValue("@Email", user.Email);
                    cmdUser.Parameters.AddWithValue("@Phone", user.Phone);
                    cmdUser.Parameters.AddWithValue("@DateOfBirth", user.DateOfBirth);
                    cmdUser.Parameters.AddWithValue("@Gender", user.Gender);
                    cmdUser.Parameters.AddWithValue("@Address", user.Address);
                    cmdUser.Parameters.AddWithValue("@Role", user.Role);
                    cmdUser.Parameters.AddWithValue("@UserID", user.UserID);

                    cmdUser.ExecuteNonQuery();

                    // Обновление учетных данных (логин и пароль)
                    string updateCredentialQuery = @"UPDATE usercredentials 
                                             SET Login=@Login, Password=@Password
                                             WHERE UserID=@UserID";
                    MySqlCommand cmdCredential = new MySqlCommand(updateCredentialQuery, connection, transaction);
                    cmdCredential.Parameters.AddWithValue("@Login", LoginTextBox.Text.Trim());
                    cmdCredential.Parameters.AddWithValue("@Password", passwordHash);  // Сохраняем хэш пароля
                    cmdCredential.Parameters.AddWithValue("@UserID", user.UserID);

                    cmdCredential.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при обновлении пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }

        private string GetUserPassword(int userId)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Password FROM usercredentials WHERE UserID=@UserID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "";
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при получении пароля пользователя: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return "";
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
