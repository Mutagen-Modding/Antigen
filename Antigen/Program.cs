using Antigen.Services;
using Antigen.Services.Singleton;
using Avalonia;
using Microsoft.Extensions.Logging;
using ReactiveUI.Avalonia;
using Serilog;

namespace Antigen;

internal sealed class Program
{
    private static ICrashLoggingService? _crashLoggingService;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        _crashLoggingService = new CrashLoggingService();

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                _crashLoggingService.LogCrash(ex);
            }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            _crashLoggingService.LogCrash(e.Exception);
            e.SetObserved();
        };

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            _crashLoggingService.LogCrash(ex);
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
                var loggerFactory = LoggerFactory.Create(logging => logging.AddSerilog(Logging.Log.Logger, dispose: true));
                var logger = loggerFactory.CreateLogger<ObservableExceptionHandler>();
                rxBuilder.WithExceptionHandler(new ObservableExceptionHandler(logger));
            });
    }
}
