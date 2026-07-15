using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Antigen.Resources.Constants;

public static class StandardBrushes
{
    public static ISolidColorBrush? TextBrush => GetBrush("ButtonForeground");
    public static ISolidColorBrush? HighlightBrush => GetBrush("SystemAccentColor");
    public static ISolidColorBrush? DarkGrayBrush => GetBrush("SystemControlForegroundBaseMediumBrush");
    public static ISolidColorBrush? LightGrayBrush => GetBrush("SystemControlForegroundBaseMediumHighBrush");

    public static ISolidColorBrush? BackgroundBrush => GetBrush("SolidBackgroundFillColorTertiary");

    public static ISolidColorBrush ValidBrush => Brushes.ForestGreen;
    public static ISolidColorBrush InvalidBrush => Brushes.IndianRed;

    public static IBrush GetStatusBrush(bool status)
    {
        return status
            ? ValidBrush
            : InvalidBrush;
    }

    public static SolidColorBrush? GetBrush(string dynamicColorKey)
    {
        return Application.Current is not null
         && Application.Current.TryFindResource(dynamicColorKey, Application.Current.ActualThemeVariant, out var obj)
                ? obj switch
                {
                    Color color => new SolidColorBrush(color),
                    SolidColorBrush brush => brush,
                    _ => null
                }
                : null;
    }
}