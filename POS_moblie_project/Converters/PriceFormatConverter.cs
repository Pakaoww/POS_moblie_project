using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;
using System.Globalization;

namespace POS_moblie_project.Converters;

public class PriceFormatConverter : IValueConverter
{
    private static CurrencyService? _currency;
    private static CurrencyService Currency
        => _currency ??= ServiceHelper.GetService<CurrencyService>();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not decimal price) return "0.00";
        var fmt = parameter as string ?? "N2";
        return Currency.Format(price, fmt);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string s)
        {
            var clean = s.Replace(Currency.Symbol, "").Trim();
            if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
        }
        return 0m;
    }
}