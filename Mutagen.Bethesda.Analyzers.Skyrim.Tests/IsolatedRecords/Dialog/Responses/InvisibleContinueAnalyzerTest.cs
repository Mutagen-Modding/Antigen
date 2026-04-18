using Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Dialog.Responses;

using Fixture = IsolatedRecordTestFixture<InvisibleContinueAnalyzer, DialogResponses, IDialogResponsesGetter>;

public class InvisibleContinueAnalyzerTest
{
    static void RunFixture(Fixture fixture, Action<DialogResponses> prepForFix)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.Flags ??= new();
                rec.Flags.Flags |= DialogResponses.Flag.InvisibleContinue;
            },
            prepForFix,
            InvisibleContinueAnalyzer.InvisibleContinueWithoutLinkTo);
    }

    [Theory, MutagenModAutoData]
    public void AddLink(Fixture fixture)
    {
        RunFixture(fixture, rec =>
        {
            rec.LinkTo.Add(FormKeys.SkyrimSE.Skyrim.DialogTopic.BQ01RewardTopic);
        });
    }

    [Theory, MutagenModAutoData]
    public void RemoveFlag(Fixture fixture)
    {
        RunFixture(fixture, rec =>
        {
            rec.Flags!.Flags &= ~DialogResponses.Flag.InvisibleContinue;
        });
    }
}
