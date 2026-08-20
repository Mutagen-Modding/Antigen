using System.Runtime.InteropServices;
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
            startup.Start();

            desktop.MainWindow = window;
            desktop.Exit += (_, _) => startup.Shutdown();
        }

        base.OnFrameworkInitializationCompleted();
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

/// <summary>Brings the app up once the container exists, and puts it away again on exit.</summary>
public sealed class AppStartup(
    Window window,
    MainVM main,
    GuiSettingsService guiSettings,
    ShutdownService shutdown,
    ILogger<AppStartup> logger) : ISingleton
{
    public void Start()
    {
        logger.LogInformation(
            "Antigen starting - {Runtime} on {OS} with {ProcessorCount} processors",
            RuntimeInformation.FrameworkDescription,
            RuntimeInformation.OSDescription,
            Environment.ProcessorCount);

        window.DataContext = main;

        RestorePosition();
    }

    public void Shutdown()
    {
        shutdown.Save();
    }

    private void RestorePosition()
    {
        var saved = guiSettings.Current;
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
}
