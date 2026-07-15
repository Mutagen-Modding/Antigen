using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Antigen.Resources.Converter;

public sealed class SeverityToBrushConverter : IValueConverter
{
    public IBrush CTDBrush { get; set; } = Brushes.IndianRed;
    public IBrush ErrorBrush { get; set; } = Brushes.Orange;
    public IBrush WarningBrush { get; set; } = Brushes.Gold;
    public IBrush SuggestionBrush { get; set; } = Brushes.CornflowerBlue;
    public IBrush InfoBrush { get; set; } = Brushes.ForestGreen;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Severity severity)
        {
            return severity switch
            {
                Severity.CTD => CTDBrush,
                Severity.Error => ErrorBrush,
                Severity.Warning => WarningBrush,
                Severity.Suggestion => SuggestionBrush,
                _ => InfoBrush
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}