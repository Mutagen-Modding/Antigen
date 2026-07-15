using System.Globalization;
using Antigen.Resources.Constants;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Antigen.Resources.Converter;

public sealed class BoolToBrushConverter : IValueConverter
{
    public IBrush ValidBrush { get; set; } = StandardBrushes.ValidBrush;
    public IBrush ErrorBrush { get; set; } = StandardBrushes.InvalidBrush;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? ValidBrush : ErrorBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Brush brush) return brush.Equals(ValidBrush);

        return false;
    }
}