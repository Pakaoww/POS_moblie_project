using System.Globalization;

namespace POS_moblie_project.Converters;

/// <summary>
/// Returns a filled brush if the dot at position (parameter) should be filled
/// based on the current PIN length (value); otherwise returns transparent.
/// </summary>
public class PinDotConverter : IValueConverter
{
    private static readonly Brush FilledBrush = new SolidColorBrush(Color.FromArgb("#222222"));
    private static readonly Brush EmptyBrush = new SolidColorBrush(Colors.Transparent);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string pin && parameter is string positionStr
            && int.TryParse(positionStr, out var position))
        {
            return pin.Length > position ? FilledBrush : EmptyBrush;
        }
        return EmptyBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}