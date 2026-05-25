using Mutagen.Bethesda.Analyzers.Skyrim.Extensions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Noggog.Testing.Extensions;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Quests;

using Fixture = IsolatedRecordTestFixture<FragmentAnalyzerQuest, Quest, IQuestGetter>;

public class FragmentAnalyzerQuestTest
{
    [Theory, MutagenModAutoData]
    public void DuplicateFragment(Fixture fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.VirtualMachineAdapter = new();
                rec.VirtualMachineAdapter.Fragments.Add(new()
                {
                    FragmentName = "Fragment_0",
                });
                rec.VirtualMachineAdapter.Fragments.Add(new()
                {
                    FragmentName = "Fragment_0",
                });
            },
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter!.Fragments[1].FragmentName = "Fragment_1";
            },
            FragmentAnalyzerQuest.DuplicateFragment);
    }

    void AddEmptyFragmentScript(Quest quest)
    {
        quest.VirtualMachineAdapter = new();
        quest.VirtualMachineAdapter.Scripts.Add(new()
        {
            Name = "QF_MyQuest"
        });
        quest.VirtualMachineAdapter.Scripts.Add(new()
        {
            Name = "MyQuestScript"
        });
    }

    [Theory, MutagenModAutoData]
    public void EmptyFragmentRemove(Fixture fixture)
    {
        fixture.Run(
            prepForError: AddEmptyFragmentScript,
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter!.Scripts.RemoveAt(0);
            },
            FragmentAnalyzerQuest.EmptyFragment);
    }

    [Theory, MutagenModAutoData]
    public void EmptyFragmentAddEntry(Fixture fixture)
    {
        fixture.Run(
            prepForError: AddEmptyFragmentScript,
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter!.Fragments.Add(new() { FragmentName = "Fragment_0" });
            },
            FragmentAnalyzerQuest.EmptyFragment);
    }

    [Theory]
    [InlineData("QF_MyQuest_1234ABCD", "QF_MyQuest_1234ABCD")]
    [InlineData("PFX_QF_MyQuest_1234ABCD", "PFX_QF_MyQuest_1234ABCD")]
    [InlineData("NotAFragment", null)]
    public void GetFragmentName(string name, string? expected)
    {
        var entry = new ScriptEntry() { Name = name };
        FragmentAnalyzerQuest.GetFragmentScriptName(entry.AsEnumerable())
            .ShouldEqual(expected);
    }
}
