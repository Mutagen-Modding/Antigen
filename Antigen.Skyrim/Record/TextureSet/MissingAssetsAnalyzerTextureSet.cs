using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.TextureSet;

public class MissingAssetsAnalyzerTextureSet : IIsolatedRecordAnalyzer<ITextureSetGetter>
{
    private readonly MissingAssetsAnalyzerUtil _util;

    public static readonly TopicDefinition<string, string?> MissingTextureInTextureSet = MutagenTopicBuilder.FromDiscussion(
            91,
            "Missing Texture in TextureSet",
            Severity.Error)
        .WithFormatting<string, string?>("Missing texture {0} at {1}");

    private const string TextureSetDiffuseName = nameof(ITextureSet.Diffuse);
    private const string TextureSetNormalOrGlossName = "Normal/Gloss";
    private const string TextureSetEnvironmentMaskOrSubsurfaceTintName = "Environment Maks/Subsurface Tint";
    private const string TextureSetGlowOrDetailMapName = "Glow/Detail Map";
    private const string TextureSetHeightName = nameof(ITextureSet.Height);
    private const string TextureSetEnvironmentName = nameof(ITextureSet.Environment);
    private const string TextureSetMultilayerName = nameof(ITextureSet.Multilayer);
    private const string TextureSetBacklightMaskOrSpecularName = "Backlight Mask/Specular";

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingTextureInTextureSet];

    public MissingAssetsAnalyzerTextureSet(MissingAssetsAnalyzerUtil util)
    {
        _util = util;
    }

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ITextureSetGetter> param)
    {
        if (!_util.FileExistsIfNotNull(param.Record.Diffuse))
        {
            param.AddTopic(
                MissingTextureInTextureSet.Format(TextureSetDiffuseName, param.Record.Diffuse));
        }

        if (!_util.FileExistsIfNotNull(param.Record.NormalOrGloss))
        {
            param.AddTopic(
                MissingTextureInTextureSet.Format(TextureSetNormalOrGlossName, param.Record.NormalOrGloss));
        }

        if (!_util.FileExistsIfNotNull(param.Record.EnvironmentMaskOrSubsurfaceTint))
        {
            param.AddTopic(
                MissingTextureInTextureSet.Format(TextureSetEnvironmentMaskOrSubsurfaceTintName, param.Record.EnvironmentMaskOrSubsurfaceTint));
        }

        if (!_util.FileExistsIfNotNull(param.Record.GlowOrDetailMap))
        {
            param.AddTopic(
                MissingTextureInTextureSet.Format(TextureSetGlowOrDetailMapName, param.Record.GlowOrDetailMap));
        }

        if (!_util.FileExistsIfNotNull(param.Record.Height))
        {
            param.AddTopic(
                MissingTextureInTextureSet.Format(TextureSetHeightName, param.Record.Height));
        }

        if (!_util.FileExistsIfNotNull(param.Record.Environment))
        {
            param.AddTopic(
                MissingTextureInTextureSet.Format(TextureSetEnvironmentName, param.Record.Environment));
        }

        if (!_util.FileExistsIfNotNull(param.Record.Multilayer))
        {
            param.AddTopic(
                MissingTextureInTextureSet.Format(TextureSetMultilayerName, param.Record.Multilayer));
        }

        if (!_util.FileExistsIfNotNull(param.Record.BacklightMaskOrSpecular))
        {
            param.AddTopic(
                MissingTextureInTextureSet.Format(TextureSetBacklightMaskOrSpecularName, param.Record.BacklightMaskOrSpecular));
        }
    }

    IEnumerable<Func<ITextureSetGetter, object?>> IIsolatedRecordAnalyzer<ITextureSetGetter>.FieldsOfInterest()
    {
        yield return x => x.Diffuse;
        yield return x => x.NormalOrGloss;
        yield return x => x.EnvironmentMaskOrSubsurfaceTint;
        yield return x => x.GlowOrDetailMap;
        yield return x => x.Height;
        yield return x => x.Environment;
        yield return x => x.Multilayer;
        yield return x => x.BacklightMaskOrSpecular;
    }
}
