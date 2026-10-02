using Antigen.Skyrim.Record.Quest;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.ContextualRecords.Quests;

public class StoryManagerQuestAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void UnassignedStoryManagerQuest(ContextualRecordTestFixture<StoryManagerQuestAnalyzer, Quest, IQuestGetter> fixture, RecordType smEvent, StoryManagerQuestNode node)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.Event = smEvent;
            },
            prepForFix: (rec, mod) =>
            {
                node.Quests.Add(new() { Quest = rec.ToNullableLink() });
            },
            StoryManagerQuestAnalyzer.StoryManagerQuestNotAssigned);
    }
}
