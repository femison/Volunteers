// Converters/PasswordConverter.cs
using System;
using System.Windows.Data;

namespace Volunteers.Converters
{
    public class PasswordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new string('•', value?.ToString().Length ?? 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}