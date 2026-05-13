using Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Quests;

using Fixture = ContextualRecordTestFixture<DialogueAliasAnalyzer, Quest, IQuestGetter>;

public class DialogueAliasAnalyzerTest
{
    // Create a response that uses GetIsAliasRef on an alias that is forced NONE
    DialogResponses Setup(Fixture fixture, Quest quest, ISkyrimMod mod)
    {
        var topic = fixture.Create<DialogTopic>();
        topic.Quest.SetTo(quest);
        mod.DialogTopics.Add(topic);
        var info = fixture.Create<DialogResponses>();
        topic.Responses.Add(info);

        quest.Aliases.Add(new QuestAlias() { ID = 1, ForcedReference = new FormLinkNullable<IPlacedGetter>() });
        info.Conditions.Add(new ConditionFloat()
        {
            ComparisonValue = 1,
            Data = new GetIsAliasRefConditionData() { ReferenceAliasIndex = 1 },
        });
        return info;
    }

    // Assigning an additional voice types form list populates voice types for export
    [Theory, MutagenModAutoData]
    public void NoVoiceList(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                Setup(fixture, rec, mod);
            },
            prepForFix: (rec, mod) =>
            {
                rec.Aliases[0].VoiceTypes.SetTo(FormKeys.SkyrimSE.Skyrim.FormList.DefaultNPCVoiceTypes);
            },
            DialogueAliasAnalyzer.InvalidDialogueAlias);
    }

    // Assigning a speaker NPC exports the speakers voice type
    [Theory, MutagenModAutoData]
    public void ExplicitSpeaker(Fixture fixture)
    {
        DialogResponses? response = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                response = Setup(fixture, rec, mod);
            },
            prepForFix: (rec, mod) =>
            {
                response!.Speaker.SetTo(FormKeys.SkyrimSE.Skyrim.Npc.DA02Boethiah);
            },
            DialogueAliasAnalyzer.InvalidDialogueAlias);
    }
}
