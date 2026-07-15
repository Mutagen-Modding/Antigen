using System.Globalization;
using Avalonia.Data.Converters;

namespace Antigen.Resources.Converter;

public sealed class BoolToDoubleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool b) return 0.0;

        if (parameter is double d)
        {
            return b ? d : 0.0;
        }

        return b ? 1.0 : 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}