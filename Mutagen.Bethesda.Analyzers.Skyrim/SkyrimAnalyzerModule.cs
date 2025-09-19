using Autofac;
using Mutagen.Bethesda.Analyzers.SDK;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.Skyrim.Record;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Noggog.Autofac;

namespace Mutagen.Bethesda.Analyzers.Skyrim;

public class SkyrimAnalyzerModule : Module, IAnalyzerModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(ConditionAnalyzer).Assembly)
            .AssignableTo<IAnalyzer>()
            .AsImplementedInterfaces()
            .SingleInstance();
        builder.RegisterAssemblyTypes(typeof(MissingAssetsAnalyzerUtil).Assembly)
            .InNamespacesOf(typeof(MissingAssetsAnalyzerUtil))
            .AsSelf()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}
