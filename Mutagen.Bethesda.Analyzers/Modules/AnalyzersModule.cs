using Autofac;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Services;
using Noggog.Autofac;

namespace Mutagen.Bethesda.Analyzers.Modules;

public class AnalyzersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<MainModule>();

        builder.RegisterAssemblyTypes(typeof(AnalyzerRunner).Assembly)
            .InNamespacesOf(
                typeof(AnalyzerRunner))
            .AsImplementedInterfaces()
            .AsSelf()
            .SingleInstance();
    }
}
