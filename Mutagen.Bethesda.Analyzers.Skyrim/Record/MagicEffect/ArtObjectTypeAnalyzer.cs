using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.MagicEffect;

public class ArtTypeAnalyzer: IContextualRecordAnalyzer<IMagicEffectGetter>
{
    public static readonly TopicDefinition<IArtObjectGetter?> IncorrectCastingArtObjectList = MutagenTopicBuilder.FromDiscussion(
            628,
            "Incorrect CastingArtObject in MagicEffect",
            Severity.Warning)
        .WithFormatting<IArtObjectGetter?>("ArtObject {0} has incorrect Art Type for MagicEffect CastingArt");

    public static readonly TopicDefinition<IArtObjectGetter?> IncorrectEnchantArtObjectList = MutagenTopicBuilder.FromDiscussion(
            628,
            "Incorrect EnchantArtObject in MagicEffect",
            Severity.Warning)
        .WithFormatting<IArtObjectGetter?>("ArtObject {0} has incorrect Art Type for MagicEffect EnchantArt");

    public static readonly TopicDefinition<IArtObjectGetter?> IncorrectHitEffectArtObjectList = MutagenTopicBuilder.FromDiscussion(
            628,
            "Incorrect HitEffectArtObject in MagicEffect",
            Severity.Warning)
        .WithFormatting<IArtObjectGetter?>("ArtObject {0} has incorrect Art Type for MagicEffect HitEffectArt");

    public IEnumerable<TopicDefinition> Topics { get; } = [IncorrectCastingArtObjectList,IncorrectEnchantArtObjectList, IncorrectHitEffectArtObjectList];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IMagicEffectGetter> param)
    {
        var mgef = param.Record;

        if ((mgef.CastingArt is null) && (mgef.EnchantArt is null) && (mgef.HitEffectArt is null)) return;

        //DOESN'T WORK, the mgef.Art.Type is either read wrong, or ArtObject.TypeEnum is wrong
        //actual value is always one behind
        //Casting is 0 instead of expected Casting
        //Hit is Casting instead of Hit
        //Enchant is Hit instead of Enchant

        if (mgef.CastingArt?.TryResolve<IArtObjectGetter>(param.LinkCache) is not null)
        {
            if (mgef.CastingArt.TryResolve<IArtObjectGetter>(param.LinkCache)!.Type != ArtObject.TypeEnum.MagicCasting)
            {
                param.AddTopic(IncorrectCastingArtObjectList.Format(mgef.CastingArt.TryResolve(param.LinkCache)));
            }
        }
        if (mgef.EnchantArt?.TryResolve<IArtObjectGetter>(param.LinkCache) is not null)
        {
            if (mgef.EnchantArt.TryResolve<IArtObjectGetter>(param.LinkCache)!.Type != ArtObject.TypeEnum.EnchantmentEffect)
            {
                param.AddTopic(IncorrectEnchantArtObjectList.Format(mgef.EnchantArt.TryResolve(param.LinkCache)));
            }
        }
        if (mgef.HitEffectArt?.TryResolve<IArtObjectGetter>(param.LinkCache) is not null)
        {
            if (mgef.HitEffectArt.TryResolve<IArtObjectGetter>(param.LinkCache)!.Type != ArtObject.TypeEnum.MagicHitEffect)
            {
                Console.WriteLine("{0} check {1} real", ArtObject.TypeEnum.MagicHitEffect, mgef.HitEffectArt.TryResolve<IArtObjectGetter>(param.LinkCache)!.Type);
                Console.WriteLine(mgef.HitEffectArt.TryResolve<IArtObjectGetter>(param.LinkCache)!.Type);
                param.AddTopic(IncorrectHitEffectArtObjectList.Format(mgef.HitEffectArt.TryResolve(param.LinkCache)));
            }
        }
    }

    public IEnumerable<Func<IMagicEffectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x;
    }
}
