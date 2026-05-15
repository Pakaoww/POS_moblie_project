using System.Globalization;

namespace POS_moblie_project.Converters;

public class BoolToTabColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive && isActive)
            return Color.FromArgb("#4ECDC4");
        return Color.FromArgb("#F5F5F7");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}