using Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Dialog.Responses;

using Fixture = IsolatedRecordTestFixture<FragmentAnalyzerResponses, DialogResponses, IDialogResponsesGetter>;

public class FragmentAnalyzerResponsesTest
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
                        OnBegin = new() { FragmentName = "Fragment_0" },
                        OnEnd = new() { FragmentName = "fragment_0" }
                    }
                };
            },
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter!.ScriptFragments!.OnEnd!.FragmentName = "Fragment_1";
            },
            FragmentAnalyzerResponses.DuplicateFragment);
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
                    OnBegin = new() { FragmentName = "Fragment_0" }
                };
            },
            FragmentAnalyzerResponses.EmptyFragment);
    }
}
