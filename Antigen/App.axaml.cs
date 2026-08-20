using System.Runtime.InteropServices;
using Antigen.Models.Settings;
using Antigen.Modules;
using Antigen.Services;
using Antigen.ViewModels;
using Antigen.Views;
using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;

namespace Antigen;

public sealed class App : Application
{
    public static IContainer? Container { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = new MainWindow();
            Container = SetupServices(window);

            var startup = Container.Resolve<AppStartup>();

            startup.Logger.LogInformation(
                "Antigen starting - {Runtime} on {OS} with {ProcessorCount} processors",
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSDescription,
                Environment.ProcessorCount);

            window.DataContext = startup.Main;

            RestorePosition(window, startup.GuiSettings.Current);

            desktop.MainWindow = window;
            desktop.Exit += (_, _) => startup.Shutdown.Save();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RestorePosition(MainWindow window, GuiSettings saved)
    {
        if (saved.WindowX is { } x && saved.WindowY is { } y
            && window.Screens.All.Any(s => s.Bounds.Contains(new PixelPoint(x, y))))
        {
            window.Position = new PixelPoint(x, y);
            return;
        }

        if (window.Screens.Primary is { } screen)
        {
            window.Position = new PixelPoint(
                screen.WorkingArea.X + (screen.WorkingArea.Width - (int)window.Width) / 2,
                screen.WorkingArea.Y + (screen.WorkingArea.Height - (int)window.Height) / 2
            );
        }
    }

    private static IContainer SetupServices(MainWindow window)
    {
        var builder = new ContainerBuilder();

        builder.RegisterInstance(window)
            .As<IMainWindow>()
            .As<Window>();

        builder.RegisterModule<MainModule>();

        return builder.Build();
    }
}

public sealed record AppStartup(
    ILogger<App> Logger,
    MainVM Main,
    GuiSettingsService GuiSettings,
    ShutdownService Shutdown) : ISingleton;
