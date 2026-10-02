using Autofac;
using Antigen.Cli.Overrides;
using Antigen.Config.Run;
using Antigen.Reporting.Handlers;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Order.DI;

namespace Antigen.Cli.Modules;

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
            builder.RegisterType<EmptyPluginListingsPathProvider>().As<IPluginListingsPathProvider>();
            builder.RegisterType<EmptyCreationClubListingsPathProvider>().As<ICreationClubListingsPathProvider>();
        }

        if (runConfig.OutputFilePath is not null)
        {
            builder.RegisterType<CsvReportHandler>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new CsvInputs(runConfig.OutputFilePath)).AsSelf().AsImplementedInterfaces();
        }

        builder.RegisterInstance(new InjectedBlacklistedModsProvider(runConfig.BlacklistMods ?? [])).As<IBlacklistedModsProvider>();
    }
}
