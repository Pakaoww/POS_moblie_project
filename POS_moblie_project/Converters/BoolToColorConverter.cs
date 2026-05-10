using System.Globalization;

namespace POS_moblie_project.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOn)
        {
            return isOn ? Color.FromArgb("#4ECDC4") : Color.FromArgb("#CCCCCC");
        }
        return Color.FromArgb("#CCCCCC");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}