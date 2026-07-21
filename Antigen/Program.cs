using Antigen.Services;
using Avalonia;
using Microsoft.Extensions.Logging;
using ReactiveUI.Avalonia;
using Serilog;

namespace Antigen;

internal sealed class Program
{
    private static readonly ILoggerFactory LoggerFactory =
        Microsoft.Extensions.Logging.LoggerFactory.Create(logging => logging.AddSerilog(Logging.Log.Logger, dispose: true));

    private static readonly ILogger<Program> Logger = LoggerFactory.CreateLogger<Program>();

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                Logger.LogCritical(ex, "Unhandled exception");
            }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Logger.LogCritical(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Unhandled exception");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithDeveloperTools()
            .LogToTrace()
            .UseReactiveUI(rxBuilder =>
            {
                var logger = LoggerFactory.CreateLogger<ObservableExceptionHandler>();
                rxBuilder.WithExceptionHandler(new ObservableExceptionHandler(logger));
            });
    }
}
