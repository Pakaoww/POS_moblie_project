using System.Globalization;
using Microsoft.Maui.Controls;

namespace POS_moblie_project.Converters;

/// <summary>
/// true (ON)  → Column 1  (thumb อยู่ขวา)
/// false (OFF) → Column 0  (thumb อยู่ซ้าย)
/// </summary>
public class BoolToThumbColumnConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? 1 : 0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// true (ON)  → Column 0  (label "ON" อยู่ซ้าย ตรงข้าม thumb ขวา)
/// false (OFF) → Column 1  (label "OFF" อยู่ขวา ตรงข้าม thumb ซ้าย)
/// </summary>
public class BoolToLabelColumnConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && b ? 0 : 1;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// true (ON)  → Center  (ข้อความ "ON" จัดกึ่งกลางคอลัมน์ซ้าย)
/// false (OFF) → Center  (ข้อความ "OFF" จัดกึ่งกลางคอลัมน์ขวา)
/// </summary>
public class BoolToLabelAlignConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => LayoutOptions.Center;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}