using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class AliasConditionAnalyzerTest
{
    [Theory, ConditionAnalyzerAutoData]
    public void AliasWithoutOwningQuest(Fixture fixture, Quest quest)
    {
        fixture.Run(prepForError: (rec, mod) =>
        {
            rec.Conditions.Add(new ConditionFloat()
            {
                Data = new GetDeadConditionData() { RunOnType = Condition.RunOnType.QuestAlias }
            });
        },
        prepForFix: (rec, mod) =>
        {
            rec.OwnerQuest.SetTo(quest);
            quest.Aliases.Add(new() { ID = 1 });
            rec.Conditions[0].Data.RunOnTypeIndex = (int)quest.Aliases[0].ID;
        },
        AliasConditionAnalyzer.AliasWithoutQuest);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void AliasWithoutOwningQuestGetIsAliasRef(Fixture fixture, Quest quest)
    {
        fixture.Run(prepForError: (rec, mod) =>
        {
            rec.Conditions.Add(new ConditionFloat()
            {
                Data = new GetIsAliasRefConditionData() { RunOnType = Condition.RunOnType.Subject, ReferenceAliasIndex = 1 }
            });
        },
        prepForFix: (rec, mod) =>
        {
            rec.OwnerQuest.SetTo(quest);
            quest.Aliases.Add(new() { ID = 1 });
        },
        AliasConditionAnalyzer.AliasWithoutQuest);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void InvalidAliasIndex(Fixture fixture, Quest quest)
    {
        fixture.Run(prepForError: (rec, mod) =>
        {
            rec.OwnerQuest.SetTo(quest);
            quest.Aliases.Add(new() { ID = 1 });

            rec.Conditions.Add(new ConditionFloat()
            {
                Data = new GetDeadConditionData() { RunOnType = Condition.RunOnType.QuestAlias, RunOnTypeIndex = 12345 }
            });
        },
        prepForFix: (rec, mod) =>
        {
            rec.Conditions[0].Data.RunOnTypeIndex = (int)quest.Aliases[0].ID;
        },
        AliasConditionAnalyzer.InvalidAliasIndex);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void InvalidAliasIndexGetIsAliasRef(Fixture fixture, Quest quest)
    {
        fixture.Run(prepForError: (rec, mod) =>
        {
            rec.OwnerQuest.SetTo(quest);
            quest.Aliases.Add(new() { ID = 1 });

            rec.Conditions.Add(new ConditionFloat()
            {
                Data = new GetIsAliasRefConditionData() { RunOnType = Condition.RunOnType.Subject, ReferenceAliasIndex = 12345 }
            });
        },
        prepForFix: (rec, mod) =>
        {
            (rec.Conditions[0].Data as IGetIsAliasRefConditionData)!.ReferenceAliasIndex = (int)quest.Aliases[0].ID;
        },
        AliasConditionAnalyzer.InvalidAliasIndex);
    }

    // Invalid alias may coexist with other topics on the same condition
    [Theory, ConditionAnalyzerAutoData]
    public void InvalidAliasAndOtherError(Fixture fixture, Quest quest)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.OwnerQuest.SetTo(quest);

                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = new GetIsAliasRefConditionData() { ReferenceAliasIndex = 1, RunOnType = Condition.RunOnType.Reference },
                });
            },
            prepForFix: (rec, mod) =>
            {
                var data = rec.Conditions[0].Data as GetIsAliasRefConditionData;
                data!.Reference.SetTo(FormKeys.SkyrimSE.Skyrim.PlacedNpc.AlvorREF);
                quest.Aliases.Add(new() { ID = (ushort)data.ReferenceAliasIndex });
            },
            InvalidConditionReferenceAnalyzer.InvalidConditionReference,
            AliasConditionAnalyzer.InvalidAliasIndex);
    }
}
