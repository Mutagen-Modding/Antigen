using Antigen.Resources.Converter;
using Antigen.Services.Game;
using Autofac;
using Mutagen.Bethesda.Analyzers.Skyrim;

namespace Antigen.Modules;

public sealed class SkyrimModule : GameCategoryModule
{
    protected override void RegisterFormattedTopicConverters(ContainerBuilder builder) =>
        builder.RegisterType<SkyrimFormattedTopicConverters>().As<IFormattedTopicConverters>();

    protected override void RegisterAnalyzerResultInfoFactory(ContainerBuilder builder) =>
        builder.RegisterType<SkyrimAnalyzerResultInfoFactory>().As<IAnalyzerResultInfoFactory>();

    protected override void RegisterAnalyzerFilter(ContainerBuilder builder) =>
        builder.RegisterType<SkyrimAnalyzerFilter>().As<IAnalyzerFilter>();

    protected override void RegisterAnalyzers(ContainerBuilder builder) =>
        builder.RegisterModule<SkyrimAnalyzerModule>();
}
