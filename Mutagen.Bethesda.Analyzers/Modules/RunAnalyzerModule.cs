using Autofac;
using Mutagen.Bethesda.Analyzers.Autofac;

namespace Mutagen.Bethesda.Analyzers.Modules;

public class RunAnalyzerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<MainModule>();
        builder.RegisterModule<ConfigModule>();
    }
}
