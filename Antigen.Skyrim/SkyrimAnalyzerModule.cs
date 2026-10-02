using Autofac;
using Antigen.SDK;
using Antigen.SDK.Analyzers;
using Antigen.Skyrim.Record.Conditions;
using Antigen.Skyrim.Util;
using Noggog.Autofac;

namespace Antigen.Skyrim;

public class SkyrimAnalyzerModule : Module, IAnalyzerModule
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(ConditionAnalyzer).Assembly)
            .AssignableTo<IAnalyzer>()
            .AsImplementedInterfaces()
            .SingleInstance();
        builder.RegisterAssemblyTypes(typeof(ConditionAnalyzer).Assembly)
            .AssignableTo<IConditionAnalyzer>()
            .AsImplementedInterfaces()
            .SingleInstance();
        builder.RegisterAssemblyTypes(typeof(MissingAssetsAnalyzerUtil).Assembly)
            .InNamespacesOf(typeof(MissingAssetsAnalyzerUtil))
            .AsSelf()
            .AsImplementedInterfaces()
            .SingleInstance();
    }
}
