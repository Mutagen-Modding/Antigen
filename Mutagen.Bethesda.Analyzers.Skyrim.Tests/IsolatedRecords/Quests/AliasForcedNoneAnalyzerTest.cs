using Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;


namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Quests
{
    using Fixtue = IsolatedRecordTestFixture<AliasForcedNoneAnalyzer, Quest, IQuestGetter>;
    public class AliasForcedNoneAnalyzerTest
    {
        // Most tests use the same setup, and differ only in how the issue is resolved
        static void RunFixture(
            Fixtue fixture, Action<Quest> prepForFix)
        {
            fixture.Run(
                prepForError: rec =>
                {
                    rec.Aliases.Add(new() { ID = 0 });
                },
                prepForFix,
                AliasForcedNoneAnalyzer.AliasForcedNone);
        }

        // Test case for each way to resolve the error
        [Theory, MutagenModAutoData]
        public void TestAliasOptional(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].Flags = QuestAlias.Flag.Optional;
                });
        }

        [Theory, MutagenModAutoData]
        public void TestAliasForcedReference(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].ForcedReference.SetTo(FormKeys.SkyrimSE.Skyrim.PlacedNpc.NazeemREF);
                });
        }

        [Theory, MutagenModAutoData]
        public void TestAliasUniqueActor(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].UniqueActor.SetTo(FormKeys.SkyrimSE.Skyrim.Npc.Jzargo);
                });
        }

        [Theory, MutagenModAutoData]
        public void TestAliasFromLocation(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].Location = new()
                    {
                        AliasID = 0,
                        RefType = FormKeys.SkyrimSE.Skyrim.LocationReferenceType.Boss.AsNullable()
                    };
                });
        }

        [Theory, MutagenModAutoData]
        public void TestAliasFromExternal(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].External = new()
                    {
                        AliasID = 0,
                        Quest = FormKeys.SkyrimSE.Skyrim.Quest.MQ101.AsNullable(),
                    };
                });
        }

        [Theory, MutagenModAutoData]
        public void TestAliasCreateReference(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].CreateReferenceToObject = new()
                    {
                        AliasID = 0,
                        Create = CreateReferenceToObject.CreateEnum.In,
                        Object = FormKeys.SkyrimSE.Skyrim.MiscItem.Gold001
                    };
                });
        }

        [Theory, MutagenModAutoData]
        public void TestAliasFindEvent(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].FindMatchingRefFromEvent = new()
                    {
                        FromEvent = new RecordType("SCPT"),
                        // Script event - Ref1
                        EventData = new byte[] { 0x52, 0x31, 0x00, 0x00 }
                    };
                });
        }





        [Theory, MutagenModAutoData]
        public void TestAliasFindNearAlias(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].FindMatchingRefNearAlias = new()
                    {
                        AliasID = 0,
                        Type = FindMatchingRefNearAlias.TypeEnum.LinkedRefChild,
                    };
                });
        }

        [Theory, MutagenModAutoData]
        public void TestAliasFindConditions(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    var data = new GetIsIDConditionData();
                    data.Object.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Npc.Alvor);
                    rec.Aliases[0].Conditions.Add(new ConditionFloat() {
                        Data = data,
                        CompareOperator = CompareOperator.EqualTo,
                        ComparisonValue = 1,
                    });
                });
        }

        [Theory, MutagenModAutoData]
        public void TestAliasSpecificLocation(Fixtue fixture)
        {
            RunFixture(fixture,
                rec =>
                {
                    rec.Aliases[0].SpecificLocation.SetTo(FormKeys.SkyrimSE.Skyrim.Location.RiverwoodLocation);
                });
        }

        [Theory, MutagenModAutoData]
        public void TestForceIntoAlias(Fixtue fixture)
        {
            fixture.Run(
                prepForError: rec =>
                {
                    rec.Aliases.Add(new()
                    {
                        ID = 0,
                        ForcedReference = FormKeys.SkyrimSE.Skyrim.PlacedNpc.UlfricREF.AsNullable()
                    });
                    rec.Aliases.Add(new()
                    {
                        ID = 1,
                    });
                },
                prepForFix: rec =>
                {
                    rec.Aliases[0].AliasIDToForceIntoWhenFilled = 1;
                },
                AliasForcedNoneAnalyzer.AliasForcedNone
            );
        }
    }
}
