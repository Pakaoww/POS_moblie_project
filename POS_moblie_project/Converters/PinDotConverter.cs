using POS_moblie_project.ViewModels;
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

/// <summary>
/// ระบาย step dot indicator (3 จุด)
/// ConverterParameter = index ของ step (0, 1, 2)
/// CurrentStep >= index → สีส้ม (#FF6B35) | ไม่งั้น → เทา (#DDDDDD)
/// </summary>
public class StepDotConverter : IValueConverter
{
    private static readonly Brush ActiveBrush = new SolidColorBrush(Color.FromArgb("#FF6B35"));
    private static readonly Brush InactiveBrush = new SolidColorBrush(Color.FromArgb("#DDDDDD"));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PinStep currentStep && parameter is string indexStr
            && int.TryParse(indexStr, out var index))
        {
            return (int)currentStep >= index ? ActiveBrush : InactiveBrush;
        }
        return InactiveBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}