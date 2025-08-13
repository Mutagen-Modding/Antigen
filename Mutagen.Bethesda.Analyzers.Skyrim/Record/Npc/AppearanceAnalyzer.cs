using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class AppearanceAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<IHeadPartGetter, IRaceGetter> InvalidHeadPart = MutagenTopicBuilder.DevelopmentTopic(
            "Npc uses invalid Head Part",
            Severity.Warning)
        .WithFormatting<IHeadPartGetter, IRaceGetter>("Npc uses head part {0} which cannot be used for the npc's race {1}");

    public static readonly TopicDefinition<IFormLinkNullableGetter<IColorRecordGetter>, IRaceGetter> InvalidHairColor = MutagenTopicBuilder.DevelopmentTopic(
            "Npc uses invalid Hair Color",
            Severity.Warning)
        .WithFormatting<IFormLinkNullableGetter<IColorRecordGetter>, IRaceGetter>("Npc uses hair color {0} which is not available for the npc's race {1}");

    public static readonly TopicDefinition<uint, IRaceGetter> InvalidNose = MutagenTopicBuilder.DevelopmentTopic(
            "Npc uses invalid Nose",
            Severity.Warning)
        .WithFormatting<uint, IRaceGetter>("Npc uses a nose {0} which is not available for the npc's race {1}");

    public static readonly TopicDefinition<uint, IRaceGetter> InvalidEye = MutagenTopicBuilder.DevelopmentTopic(
            "Npc uses invalid Eye",
            Severity.Warning)
        .WithFormatting<uint, IRaceGetter>("Npc uses eyes {0} which is not available for the npc's race {1}");

    public static readonly TopicDefinition<uint, IRaceGetter> InvalidLip = MutagenTopicBuilder.DevelopmentTopic(
            "Npc uses invalid Lip",
            Severity.Warning)
        .WithFormatting<uint, IRaceGetter>("Npc uses lips {0} which is not available for the npc's lip {1}");

    public static readonly TopicDefinition<ushort?, IRaceGetter> MissingTintLayer = MutagenTopicBuilder.DevelopmentTopic(
            "Npc uses invalid Tint Layer",
            Severity.Error)
        .WithFormatting<ushort?, IRaceGetter>("Npc uses a tint layer {0} which is not available for the npc's race {1}");

    public static readonly TopicDefinition<IFormLinkNullableGetter<ITextureSetGetter>, IRaceGetter> InvalidHeadTexture = MutagenTopicBuilder.DevelopmentTopic(
            "Npc uses invalid Head Texture",
            Severity.Error)
        .WithFormatting<IFormLinkNullableGetter<ITextureSetGetter>, IRaceGetter>("Npc uses head texture {0} which is not available for the npc's race {1}");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        InvalidHeadPart,
        InvalidHairColor,
        InvalidNose,
        InvalidEye,
        InvalidLip,
        MissingTintLayer,
        InvalidHeadTexture
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (npc.IsDeleted) return;

        var race = npc.Race.TryResolve(param.LinkCache);
        if (race?.HeadData is null || !race.Flags.HasFlag(Race.Flag.FaceGenHead)) return;

        var maleFemaleGender = npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.Female) ? MaleFemaleGender.Female : MaleFemaleGender.Male;
        var headData = race.HeadData[maleFemaleGender];
        if (headData is null) return;

        // Hair Color
        if (!npc.HairColor.FormKey.IsNull && headData.AvailableHairColors.All(c => c.FormKey != npc.HairColor.FormKey))
        {
            param.AddTopic(
                InvalidHairColor.Format(npc.HairColor, race));
        }

        if (npc.FaceParts is not null)
        {
            if (!headData.IsNoseMorphAvailable(npc.FaceParts.Nose))
            {
                param.AddTopic(
                    InvalidNose.Format(npc.FaceParts.Nose, race));
            }

            if (!headData.IsEyeMorphAvailable(npc.FaceParts.Eyes))
            {
                param.AddTopic(
                    InvalidEye.Format(npc.FaceParts.Eyes, race));
            }

            if (!headData.IsLipMorphAvailable(npc.FaceParts.Mouth))
            {
                param.AddTopic(
                    InvalidLip.Format(npc.FaceParts.Mouth, race));
            }
        }

        foreach (var headPartLink in npc.HeadParts)
        {
            var headPart = headPartLink.TryResolve(param.LinkCache);
            if (headPart is null) continue;

            CheckHeadPart(headPart);
        }

        foreach (var tintLayer in npc.TintLayers)
        {
            var raceTintLayer = headData.TintMasks.FirstOrDefault(x => x.Index == tintLayer.Index);
            if (raceTintLayer is null)
            {
                param.AddTopic(
                    MissingTintLayer.Format(tintLayer.Index, race));
            }
        }

        if (!npc.HeadTexture.FormKey.IsNull && headData.FaceDetails.All(f => f.FormKey != npc.HeadTexture.FormKey))
        {
            param.AddTopic(
                InvalidHeadTexture.Format(npc.HeadTexture, race));
        }

        void CheckHeadPart(IHeadPartGetter headPart)
        {
            var formList = headPart.ValidRaces.TryResolve(param.LinkCache);
            if (formList is not null && formList.Items.All(x => x.FormKey != race.FormKey))
            {
                param.AddTopic(
                    InvalidHeadPart.Format(headPart, race));
            }

            foreach (var extraPartLink in headPart.ExtraParts)
            {
                var extraPart = extraPartLink.TryResolve(param.LinkCache);
                if (extraPart is null) continue;

                CheckHeadPart(extraPart);
            }
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.FaceParts;
        yield return x => x.TintLayers;
        yield return x => x.HairColor;
        yield return x => x.HeadParts;
        yield return x => x.HeadTexture;
        yield return x => x.Race;
    }
}
