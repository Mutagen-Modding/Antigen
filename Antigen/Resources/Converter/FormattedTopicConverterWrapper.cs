using System.Globalization;
using Autofac;
using Avalonia.Data.Converters;

namespace Antigen.Resources.Converter;

public sealed class FormattedTopicConverterWrapper : IValueConverter
{

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var inner = ResolveInner();
        return inner.Convert(value, targetType, parameter, culture);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
        // No conversion back needed
    }
    private static IValueConverter ResolveInner()
    {
        var container = App.Container ?? throw new InvalidOperationException("Autofac container not initialized");
        var converters = container.Resolve<IFormattedTopicConverters>();
        return converters.ExtractMessage ?? throw new InvalidOperationException("FormattedTopicConverters not registered");
    }
}