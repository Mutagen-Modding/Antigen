using System.Globalization;
using Antigen.Resources.Constants;
using Avalonia.Data.Converters;
using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Antigen.Resources.Converter;

public sealed class SeverityToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Severity severity
            ? SeverityBrushes.Solid(severity)
            : null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}
