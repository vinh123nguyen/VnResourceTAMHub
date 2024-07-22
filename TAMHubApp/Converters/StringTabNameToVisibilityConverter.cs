using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TAMHubApp.Converters
{
    public class StringTabNameToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string && values[1] is string && (string)values[0] == "HomeView" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            else if (values[0] is string && values[1] is string && (string)values[0] == "DeviceView" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            else if (values[0] is string && values[1] is string && (string)values[0] == "DataView" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            else if (values[0] is string && values[1] is string && (string)values[0] == "ScheduleView" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            else if (values[0] is string && values[1] is string && (string)values[0] == "ActionLogView" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            else if (values[0] is string && values[1] is string && (string)values[0] == "General" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            else if (values[0] is string && values[1] is string && (string)values[0] == "ZKTeco" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            else if (values[0] is string && values[1] is string && (string)values[0] == "Realand" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            else if (values[0] is string && values[1] is string && (string)values[0] == "SettingView" && (string)values[0] == (string)values[1])
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
