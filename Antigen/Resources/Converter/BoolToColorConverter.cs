using System.Globalization;
using Antigen.Resources.Constants;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Antigen.Resources.Converter;

public sealed class BoolToColorConverter : IValueConverter
{
    public Color ValidColor { get; set; } = StandardBrushes.ValidBrush.Color;
    public Color ErrorColor { get; set; } = StandardBrushes.InvalidBrush.Color;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? ValidColor : ErrorColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color) return color.Equals(ValidColor);

        return false;
    }
}