using System.IO.Abstractions;
using Antigen.Models.Settings;
using Antigen.Modules;
using Antigen.Services.Singleton;
using Antigen.Services.Transient;
using Antigen.ViewModels.Singleton;
using Antigen.ViewModels.Transient;
using Antigen.Views;
using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Autofac;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Meta;

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

            Container.Resolve<ILogger<App>>().LogInformation("Antigen starting");

            var mainVM = Container.Resolve<MainVM>();
            window.DataContext = mainVM;

            RestorePosition(window, mainVM.SavedSettings);

            desktop.MainWindow = window;
            desktop.Exit += (_, _) => mainVM.SaveGuiSettings();
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

        builder.RegisterModule<LoggingModule>();

        // Register base services
        builder.RegisterType<FileSystem>()
            .As<IFileSystem>()
            .SingleInstance();

        builder.RegisterModule<MutagenModule>();

        builder.RegisterModule<SkyrimModule>();

        builder.RegisterModule<SkyrimAnalyzerModule>();

        builder.Register(context =>
        {
            var gameReleaseContext = context.Resolve<IGameReleaseContext>();
            return GameConstants.Get(gameReleaseContext.Release);
        });

        // Register application services and view models by folder
        builder.RegisterFolder<AnalyzerService>(RegistrationStyle.Singleton);
        builder.RegisterFolder<ModWatcher>(RegistrationStyle.Transient);
        builder.RegisterFolder<MainVM>(RegistrationStyle.Singleton);
        builder.RegisterFolder<AnalyzerVM>(RegistrationStyle.Transient);

        return builder.Build();
    }
}
