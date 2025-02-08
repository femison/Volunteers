using System;
using System.Windows;
using MySql.Data.MySqlClient;
using Volunteers.Data;

namespace Volunteers.Views
{
    public partial class EditAccountWindow : Window
    {
        private int userId;
        private string currentPassword;

        // Конструктор, который принимает UserID для редактирования
        public EditAccountWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadUserData();
        }

        private void LoadUserData()
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Login, Password FROM usercredentials WHERE UserID = @UserID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        LoginTextBox.Text = reader.GetString("Login");
                        currentPassword = reader.GetString("Password");
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных учетной записи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string newLogin = LoginTextBox.Text.Trim();
            string newPassword = PasswordBox.Password.Trim();

            // Если новый пароль не был введен, используем текущий пароль
            string passwordToSave = string.IsNullOrWhiteSpace(newPassword) ? currentPassword : newPassword;

            // Обновление учетной записи в базе данных
            if (UpdateUserCredentialsInDatabase(newLogin, passwordToSave))
            {
                MessageBox.Show("Учетная запись успешно обновлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении учетной записи.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool UpdateUserCredentialsInDatabase(string login, string password)
        {
            using (MySqlConnection connection = Database.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE usercredentials 
                                     SET Login = @Login, Password = @Password
                                     WHERE UserID = @UserID";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Login", login);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show("Ошибка при обновлении учетной записи: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
