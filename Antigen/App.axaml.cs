using System.IO.Abstractions;
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
    public static Window? MainAppWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Setup dependency injection
            Container = SetupServices();

            MainAppWindow = new MainWindow
            {
                DataContext = Container.Resolve<MainVM>()
            };

            var screen = MainAppWindow.Screens.Primary;
            if (screen is not null)
            {
                const int offset = 25;
                MainAppWindow.Position = new PixelPoint(
                    screen.WorkingArea.Right - (int)MainAppWindow.Width - offset,
                    offset
                );
            }

            desktop.MainWindow = MainAppWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IContainer SetupServices()
    {
        var builder = new ContainerBuilder();

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

        builder.RegisterType<SkyrimAnalyzerResultInfoFactory>();

        // Register ViewModels
        builder.RegisterType<MainVM>().SingleInstance();
        builder.RegisterType<AnalyzerVM>();
        builder.RegisterType<SettingsVM>();
        builder.RegisterType<ModWatcherVM>();

        return builder.Build();
    }
}