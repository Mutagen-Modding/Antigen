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
            return Convert(severity, CTDBrush, ErrorBrush, WarningBrush, SuggestionBrush, InfoBrush);
        }

        return null;
    }

    public static IBrush Convert(Severity severity, IBrush ctdBrush, IBrush errorBrush, IBrush warningBrush, IBrush suggestionBrush, IBrush infoBrush)
    {
        return severity switch
        {
            Severity.CTD => ctdBrush,
            Severity.Error => errorBrush,
            Severity.Warning => warningBrush,
            Severity.Suggestion => suggestionBrush,
            _ => infoBrush
        };
    }

    public static IBrush Convert(Severity severity)
    {
        return severity switch
        {
            Severity.CTD => Brushes.IndianRed,
            Severity.Error => Brushes.Orange,
            Severity.Warning => Brushes.Gold,
            Severity.Suggestion => Brushes.CornflowerBlue,
            _ => Brushes.ForestGreen
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}