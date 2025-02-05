using Autofac;
using Mutagen.Bethesda.Analyzers.Cli.Overrides;
using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Order.DI;

namespace Mutagen.Bethesda.Analyzers.Cli.Modules;

public class RunConfigModule(IRunConfigLookup runConfig) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        if (runConfig.DataDirectoryPath.HasValue)
        {
            var dataDirectoryProvider = new DataDirectoryInjection(runConfig.DataDirectoryPath.Value);
            builder.RegisterInstance(dataDirectoryProvider).As<IDataDirectoryProvider>();
        }

        if (runConfig.LoadOrderSetToMods is not null)
        {
            builder.RegisterInstance(new InjectedEnabledPluginListingsProvider(runConfig.LoadOrderSetToMods)).As<IEnabledPluginListingsProvider>();
            builder.RegisterType<NullPluginListingsPathProvider>().As<IPluginListingsPathProvider>();
            builder.RegisterType<NullCreationClubListingsPathProvider>().As<ICreationClubListingsPathProvider>();
        }

        if (runConfig.OutputFilePath is not null)
        {
            builder.RegisterType<CsvReportHandler>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new CsvInputs(runConfig.OutputFilePath)).AsSelf().AsImplementedInterfaces();
        }

        if (runConfig.OutputFilePath is not null)
        {
            builder.RegisterType<CsvReportHandler>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new CsvInputs(runConfig.OutputFilePath)).AsSelf().AsImplementedInterfaces();
        }
    }
}
