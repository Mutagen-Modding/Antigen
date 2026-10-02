using Autofac;
using Antigen.Drivers;
using Antigen.Drivers.RecordFrame;
using Antigen.Drivers.Records;
using Antigen.SDK.Analyzers;
using Mutagen.Bethesda.Plugins.Records.Mapping;
using Noggog;

namespace Antigen.Autofac;

public class ReflectionDriverModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var getterTypeMapper = new GetterTypeMapper(MetaInterfaceMapping.Instance);
        foreach (var analyzerType in TypeExt.GetInheritingFromGenericInterface(
                         typeof(IIsolatedRecordAnalyzer<>),
                         loadAssemblies: true)
                     .Concat(TypeExt.GetInheritingFromGenericInterface(
                         typeof(IIsolatedRecordFrameAnalyzer<>),
                         loadAssemblies: true))
                     .Concat(TypeExt.GetInheritingFromGenericInterface(
                         typeof(IContextualRecordAnalyzer<>),
                         loadAssemblies: true))
                     .Concat(TypeExt.GetInheritingFromGenericInterface(
                         typeof(IContextualRecordFrameAnalyzer<>),
                         loadAssemblies: true))
                     .Select(x => x.Key.GetGenericArguments()[0])
                     .Distinct()
                     .Select(x => getterTypeMapper.TryGetGetterType(x, out var getter) ? getter : throw new ArgumentException($"Failed to get getter type for {x}"))
                     .Distinct())
        {
            builder.RegisterType(typeof(ByGenericTypeRecordIsolatedDriver<>).MakeGenericType(analyzerType))
                .As<IIsolatedDriver>();
            builder.RegisterType(typeof(ByGenericTypeRecordContextualDriver<>).MakeGenericType(analyzerType))
                .As<IContextualDriver>();
            builder.RegisterType(typeof(ByGenericTypeRecordFrameIsolatedDriver<>).MakeGenericType(analyzerType))
                .As<IIsolatedRecordFrameAnalyzerDriver>();
            builder.RegisterType(typeof(ByGenericTypeRecordFrameContextualDriver<>).MakeGenericType(analyzerType))
                .As<IContextualRecordFrameAnalyzerDriver>();
        }
    }
}
