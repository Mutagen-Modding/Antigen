using Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Quests;

public class AliasIDAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void TestNextAliasIDAlreadyInUse(
        IsolatedRecordTestFixture<AliasIDAnalyzer, Quest, IQuestGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.Aliases.Add(new QuestAlias{ID = 0});
                rec.NextAliasID = 0;
            },
            prepForFix: rec =>
            {
                rec.Aliases.Add(new QuestAlias { ID = 0 });
                rec.NextAliasID = 1;
            },
            AliasIDAnalyzer.NextAliasIDAlreadyInUse);
    }

    [Theory, MutagenModAutoData]
    public void TestAliasIDDuplicate(
    IsolatedRecordTestFixture<AliasIDAnalyzer, Quest, IQuestGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.Aliases.Add(new QuestAlias { ID = 0 });
                rec.Aliases.Add(new QuestAlias { ID = 0 });
                rec.NextAliasID = 2;
            },
            prepForFix: rec =>
            {
                rec.Aliases.Add(new QuestAlias { ID = 0 });
                rec.Aliases.Add(new QuestAlias { ID = 1 });
                rec.NextAliasID = 2;
            },
            AliasIDAnalyzer.AliasIDDuplicate);
    }

    [Theory, MutagenModAutoData]
    public void TestAliasReferencesSelf(
    IsolatedRecordTestFixture<AliasIDAnalyzer, Quest, IQuestGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.Aliases.Add(new QuestAlias { ID = 0 });
                rec.Aliases.Add(new QuestAlias{
                    ID = 1,
                    CreateReferenceToObject = new CreateReferenceToObject
                    {
                        AliasID = 1,
                    }
                });
                rec.NextAliasID = 2;
            },
            prepForFix: rec =>
            {
                rec.Aliases.Add(new QuestAlias { ID = 0 });
                rec.Aliases.Add(new QuestAlias
                {
                    ID = 1,
                    CreateReferenceToObject = new CreateReferenceToObject { AliasID = 0 }
                });
                rec.NextAliasID = 2;
            },
            AliasIDAnalyzer.AliasReferencesSelf);
    }

    [Theory, MutagenModAutoData]
    public void TestAliasReferenceNotPreviousAlias(
    IsolatedRecordTestFixture<AliasIDAnalyzer, Quest, IQuestGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.Aliases.Add(new QuestAlias
                {
                    ID = 1,
                    Location = new LocationAliasReference { AliasID = 0 }
                });
                rec.Aliases.Add(new QuestAlias { ID = 0 });
                rec.NextAliasID = 2;
            },
            prepForFix: rec =>
            {
                rec.Aliases.Add(new QuestAlias { ID = 0 });
                rec.Aliases.Add(new QuestAlias
                {
                    ID = 1,
                    Location = new LocationAliasReference { AliasID = 0 }
                });
                rec.NextAliasID = 2;
            },
            AliasIDAnalyzer.AliasReferenceNotPreviousAlias);
    }
}
