using Antigen.Resources.Converter;
using Antigen.Services;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Modules;

public sealed class SkyrimModule : GameSpecificModule<ISkyrimMod, ISkyrimModGetter>
{
    protected override GameRelease GameRelease => GameRelease.SkyrimSE;

    protected override IReg<IModInfoProvider<ISkyrimModGetter>> ModInfoProvider => Register<SkyrimModInfoProvider>();
    protected override IReg<IFormattedTopicConverters> FormattedTopicConverters => Register<SkyrimFormattedTopicConverters>();
    protected override IReg<IAnalyzerResultInfoFactory> AnalyzerResultInfoFactory => Register<SkyrimAnalyzerResultInfoFactory>();
    protected override IReg<IAnalyzerFilter> AnalyzerFilter => Register<SkyrimAnalyzerFilter>();
}