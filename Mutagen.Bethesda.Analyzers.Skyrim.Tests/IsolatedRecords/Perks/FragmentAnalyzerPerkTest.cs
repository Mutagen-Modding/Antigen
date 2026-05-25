using Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Perk;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Perks;

using Fixture = IsolatedRecordTestFixture<FragmentAnalyzerPerk, Perk, IPerkGetter>;

public class FragmentAnalyzerPerkTest
{
    [Theory, MutagenModAutoData]
    public void DuplicateFragment(Fixture fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.VirtualMachineAdapter = new()
                {
                    ScriptFragments = new()
                    {
                        Fragments =
                        [
                            new() { FragmentName = "Fragment_0" },
                            new() { FragmentName = "fragment_0" }
                        ]
                    }
                };
            },
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter!.ScriptFragments!.Fragments[1].FragmentName = "Fragment_1";
            },
            FragmentAnalyzerQuest.DuplicateFragment);
    }

    [Theory, MutagenModAutoData]
    public void EmptyFragment(Fixture fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.VirtualMachineAdapter = new();
            },
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter = null;
            },
            FragmentAnalyzerResponses.EmptyFragment);
    }

    [Theory, MutagenModAutoData]
    public void EmptyFragmentAdd(Fixture fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.VirtualMachineAdapter = new();
            },
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter!.ScriptFragments = new()
                {
                    Fragments = [new() { FragmentName = "Fragment_0" }]
                };
            },
            FragmentAnalyzerResponses.EmptyFragment);
    }
}
