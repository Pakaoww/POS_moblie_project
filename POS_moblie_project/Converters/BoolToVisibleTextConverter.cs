using System.Globalization;

namespace POS_moblie_project.Converters;

public class BoolToVisibleTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
            return isVisible ? "ON" : "OFF";
        return "OFF";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}