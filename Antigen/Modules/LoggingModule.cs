using Antigen.Services;
using Autofac;
using Microsoft.Extensions.Logging;

namespace Antigen.Modules;

public class LoggingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
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
    }
}
