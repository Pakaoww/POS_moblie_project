using System.Globalization;
using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Converters;

/// <summary>
/// Returns true when the current CartState matches the parameter.
/// Usage: IsVisible="{Binding CurrentState, Converter={StaticResource CartStateConverter}, ConverterParameter=Cart}"
/// </summary>
public class CartStateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is CartState current && parameter is string target
            && Enum.TryParse<CartState>(target, out var targetState))
        {
            return current == targetState;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}