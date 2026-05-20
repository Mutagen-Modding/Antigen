using Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Dialog.Responses;

using Fixture = ContextualRecordTestFixture<SharedDialogueAnalyzer, DialogResponses, IDialogResponsesGetter>;

public class SharedDialogueAnalyzerTest
{
    // Add `responses` to a new shared info topic
    static void CreateSharedTopic(Fixture fixture, ISkyrimMod mod, DialogResponses responses)
    {
        var topic = fixture.Create<DialogTopic>();
        mod.DialogTopics.Add(topic);
        topic.SubtypeName = "IDAT";
        topic.Responses.Add(responses);
    }

    // Create a new response that uses `responses` as its shared response data
    static void AddResponseUser(Fixture fixture, ISkyrimMod mod, DialogResponses responses)
    {
        var topic = fixture.Create<DialogTopic>();
        mod.DialogTopics.Add(topic);
        var response = fixture.Create<DialogResponses>();
        topic.Responses.Add(response);
        response.ResponseData.SetTo(responses);
    }

    [Theory, MutagenModAutoData]
    public void ScriptInSharedDialogue(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                CreateSharedTopic(fixture, mod, rec);
                rec.VirtualMachineAdapter = new() { Scripts = [new() { Name = "TIF__MyFragment" }] };

                // Satisfy UnusedSharedDialogue
                AddResponseUser(fixture, mod, rec);
            },
            prepForFix: (rec, mod) =>
            {
                rec.VirtualMachineAdapter = null;
            },
            SharedDialogueAnalyzer.ScriptInSharedDialogue);
    }

    [Theory, MutagenModAutoData]
    public void UnusedSharedDialogue(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                CreateSharedTopic(fixture, mod, rec);
            },
            prepForFix: (rec, mod) =>
            {
                AddResponseUser(fixture, mod, rec);
            },
            SharedDialogueAnalyzer.UnusedSharedDialogue);
    }
}
