using System.Globalization;

namespace POS_moblie_project.Converters;

public class BoolToErrorColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isError && isError)
            return Color.FromArgb("#E53935"); // Red
        return Color.FromArgb("#666666");      // Default gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}