using Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Quests;

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
