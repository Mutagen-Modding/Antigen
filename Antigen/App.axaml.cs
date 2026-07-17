using System.IO.Abstractions;
using Antigen.Models.Settings;
using Antigen.Modules;
using Antigen.Services;
using Antigen.ViewModels;
using Antigen.ViewModels.Analyzer;
using Antigen.ViewModels.Settings;
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

        // Register logger factory and loggers using Microsoft.Extensions.Logging
        var loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Debug);
        });

        builder.RegisterInstance(loggerFactory)
            .As<ILoggerFactory>()
            .SingleInstance();

        // Register generic ILogger<T>
        builder.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();

        // Register crash logging service
        builder.RegisterType<CrashLoggingService>()
            .As<ICrashLoggingService>()
            .SingleInstance();

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

        // Register application services
        builder.RegisterType<AnalyzerService>()
            .As<IAnalyzerService>()
            .SingleInstance();

        builder.RegisterType<ModWatcher>()
            .As<IModWatcher>();

        builder.RegisterType<SkyrimModInfoProvider>()
            .As<IModInfoProvider>();

        builder.RegisterType<SettingsService>()
            .As<ISettingsService>()
            .SingleInstance();

        builder.RegisterType<GuiSettingsService>()
            .SingleInstance();

        builder.RegisterType<SkyrimAnalyzerResultInfoFactory>();

        // Register ViewModels
        builder.RegisterType<MainVM>().SingleInstance();
        builder.RegisterType<HomeVM>().SingleInstance();
        builder.RegisterType<AnalyzerVM>();
        builder.RegisterType<SettingsVM>();
        builder.RegisterType<ModWatcherVM>();

        return builder.Build();
    }
}
