
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Npcs;

public class AmbushAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void TestAmbushUnaggressive(
        IsolatedRecordTestFixture<AmbushAnalyzer, Npc, INpcGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.EditorID = "TestActorAmbush";
                rec.VirtualMachineAdapter = new VirtualMachineAdapter();
                rec.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                {
                    Name = "masterambushscript"
                });
                rec.AIData = new AIData()
                {
                    Aggression = Aggression.Aggressive
                };
            },
            prepForFix: rec =>
            {
                rec.EditorID = "TestActorAmbush";
                rec.VirtualMachineAdapter = new VirtualMachineAdapter();
                rec.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                {
                    Name = "masterambushscript"
                });
                rec.AIData = new AIData()
                {
                    Aggression = Aggression.Unaggressive
                };
            },
            AmbushAnalyzer.AmbushAggressive);
    }

    [Theory, MutagenModAutoData]
    public void TestAmbushMissingScript(
    IsolatedRecordTestFixture<AmbushAnalyzer, Npc, INpcGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.EditorID = "TestActorAmbush";
                rec.AIData = new AIData()
                {
                    Aggression = Aggression.Unaggressive
                };
            },
            prepForFix: rec =>
            {
                rec.EditorID = "TestActorAmbush";
                rec.VirtualMachineAdapter = new VirtualMachineAdapter();
                rec.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                {
                    Name = "masterambushscript"
                });
                rec.AIData = new AIData()
                {
                    Aggression = Aggression.Unaggressive
                };
            },
            AmbushAnalyzer.AmbushMissingScript);
    }

    [Theory, MutagenModAutoData]
    public void TestAmbushNotInEditorId(
    IsolatedRecordTestFixture<AmbushAnalyzer, Npc, INpcGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.EditorID = "TestActor";
                rec.VirtualMachineAdapter = new VirtualMachineAdapter();
                rec.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                {
                    Name = "masterambushscript"
                });
                rec.AIData = new AIData()
                {
                    Aggression = Aggression.Unaggressive,
                };
            },
            prepForFix: rec =>
            {
                rec.EditorID = "TestActorAmbush";
                rec.VirtualMachineAdapter = new VirtualMachineAdapter();
                rec.VirtualMachineAdapter.Scripts.Add(new ScriptEntry()
                {
                    Name = "masterambushscript"
                });
                rec.AIData = new AIData()
                {
                    Aggression = Aggression.Unaggressive
                };
            },
            AmbushAnalyzer.AmbushNotInEditorId);
    }
}
