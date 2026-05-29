using Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Scene;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Scenes;

using Fixture = IsolatedRecordTestFixture<FragmentAnalyzerScene, Scene, ISceneGetter>;

public class FragmentAnalyzerSceneTest
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
                        PhaseFragments =
                        [
                            new() { FragmentName = "Fragment_0" },
                            new() { FragmentName = "fragment_0" }
                        ]
                    }
                };
            },
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter!.ScriptFragments!.PhaseFragments[1].FragmentName = "Fragment_1";
            },
            FragmentAnalyzerScene.DuplicateFragment);
    }

    [Theory, MutagenModAutoData]
    public void DuplicateFragmentOnBeginEnd(Fixture fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.VirtualMachineAdapter = new()
                {
                    ScriptFragments = new()
                    {
                        PhaseFragments = [new() { FragmentName = "Fragment_0" }],
                        OnBegin = new() { FragmentName = "fragment_0" },
                        OnEnd = new() { FragmentName = "FRAGMENT_0" },
                    },
                };
            },
            prepForFix: rec =>
            {
                rec.VirtualMachineAdapter!.ScriptFragments!.OnBegin!.FragmentName = "Fragment_1";
                rec.VirtualMachineAdapter!.ScriptFragments!.OnEnd!.FragmentName = "Fragment_2";
            },
            FragmentAnalyzerScene.DuplicateFragment);
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
