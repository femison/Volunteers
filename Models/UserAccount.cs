// Models/UserAccount.cs
using System.ComponentModel;

namespace Volunteers.Models
{
    public class UserAccount : INotifyPropertyChanged
    {
        private bool _isPasswordVisible;

        public int UserID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set
            {
                _isPasswordVisible = value;
                OnPropertyChanged(nameof(DisplayPassword));
            }
        }

        public string DisplayPassword => IsPasswordVisible ? Password : new string('•', Password.Length);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}