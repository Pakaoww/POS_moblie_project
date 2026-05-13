using System.Globalization;

namespace POS_moblie_project.Converters;

public class DiscountActiveColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isActive = value is bool b && b;
        bool isBg = parameter as string == "bg";
        if (isBg)
            return isActive ? Color.FromArgb("#F4A620") : Color.FromArgb("#E8E8E8");
        else
            return isActive ? Colors.White : Color.FromArgb("#888888");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}