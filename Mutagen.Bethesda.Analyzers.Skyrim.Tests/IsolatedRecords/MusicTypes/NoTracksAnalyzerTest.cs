using DynamicData;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.MusicType;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.MusicTypes;

public  class NoTracksAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void NoTracks(
        IsolatedRecordTestFixture<NoTracksAnalyzer, MusicType, IMusicTypeGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.EditorID = "TestMusicType",
            prepForFix: static rec =>
            {
                rec.EditorID = "TestMusicType";
                rec.Tracks = new ExtendedList<Plugins.IFormLinkGetter<IMusicTrackGetter>>();
                rec.Tracks.Add(FormKeys.SkyrimSE.Skyrim.MusicTrack.MUSCombat01);
            },
            NoTracksAnalyzer.NoTracks);
    }
}
