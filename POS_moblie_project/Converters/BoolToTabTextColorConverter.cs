using System.Globalization;

namespace POS_moblie_project.Converters;

public class BoolToTabTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive && isActive)
            return Colors.White;
        return Color.FromArgb("#666666");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}