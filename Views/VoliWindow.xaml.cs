// Views/VoliWindow.xaml.cs
using System.Windows;
using Volunteers.Models;

namespace Volunteers.Views
{
    public partial class VoliWindow : Window
    {
        private User currentUser;

        public VoliWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            // Дополнительная инициализация, если необходимо
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (this.Owner != null)
            {
                MainWindow mainWindow = this.Owner as MainWindow;
                if (mainWindow != null)
                {
                    // Очищаем поля логина и пароля
                    mainWindow.Show();              // Показываем окно авторизации
                }
            }
        }
    }
}
