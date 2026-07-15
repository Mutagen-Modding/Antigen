using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Antigen.Resources.Converter;

public class ListContainsConverter : AvaloniaObject;
public sealed class ListContainsConverter<T> : ListContainsConverter, IValueConverter
{
    public static readonly StyledProperty<IReadOnlyList<T>> ListProperty =
        AvaloniaProperty.Register<ListContainsConverter, IReadOnlyList<T>>(nameof(List));

    public IReadOnlyList<T> List
    {
        get => GetValue(ListProperty);
        set => SetValue(ListProperty, value);
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is T t)
        {
            return List.Contains(t);
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("ListContainsConverter does not support ConvertBack.");
    }
}