using System.Runtime.InteropServices;
using Antigen.Models.Settings;
using Antigen.Modules;
using Antigen.ViewModels;
using Antigen.Views;
using Autofac;
using Avalonia;
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

            var logger = Container.Resolve<ILogger<App>>();
            logger.LogInformation(
                "Antigen starting - {Runtime} on {OS} with {ProcessorCount} processors",
                RuntimeInformation.FrameworkDescription,
                RuntimeInformation.OSDescription,
                Environment.ProcessorCount);

            var mainVM = Container.Resolve<MainVM>();
            window.DataContext = mainVM;

            RestorePosition(window, mainVM.SavedSettings);

            desktop.MainWindow = window;
            desktop.Exit += (_, _) => mainVM.Exit();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void RestorePosition(MainWindow window, GuiSettings? saved)
    {
        if (saved is not null && window.Screens.All.Any(s => s.Bounds.Contains(new PixelPoint(saved.WindowX, saved.WindowY))))
        {
            window.Position = new PixelPoint(saved.WindowX, saved.WindowY);
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
            .As<IMainWindow>();

        builder.RegisterModule<MainModule>();

        return builder.Build();
    }
}
