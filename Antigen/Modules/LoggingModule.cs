using Autofac;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Antigen.Modules;

public sealed class LoggingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var loggerFactory = LoggerFactory.Create(logging => logging.AddSerilog(Logging.Log.Logger, dispose: true));

        builder.RegisterInstance(loggerFactory)
            .As<ILoggerFactory>()
            .SingleInstance();

        builder.RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();
    }
}
