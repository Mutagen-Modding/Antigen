using AutoFixture;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.Testing.Extensions;
using Shouldly;

namespace Mutagen.Bethesda.Analyzers.Testing.Frameworks;

public class IsolatedRecordTestFixture<TAnalyzer, TMajor, TMajorGetter>
    where TMajor : IMajorRecord, TMajorGetter
    where TMajorGetter : IMajorRecordGetter
    where TAnalyzer : IIsolatedRecordAnalyzer<TMajorGetter>
{
    private readonly IFixture _fixture;
    public TAnalyzer Sut { get; }

    public IsolatedRecordTestFixture(TAnalyzer sut, IFixture fixture)
    {
        _fixture = fixture;
        Sut = sut;
    }

    public void Run(
        Action<TMajor> prepForError,
        Action<TMajor> prepForFix,
        params TopicDefinition[] expectedTopics)
    {
        var rec = _fixture.Create<TMajor>();
        prepForError(rec);

        var dropOff = new TestDropoff();
        var param = new IsolatedRecordAnalyzerParams<TMajorGetter>(
            mod: ModKey.Null,
            record: rec,
            parameters: default,
            reportDropbox: dropOff);

        Sut.AnalyzeRecord(param);
        dropOff.Reports.Select(x => x.TopicDefinition.Id)
            .ShouldEqual(expectedTopics.Select(x => x.Id));

        prepForFix(rec);

        // ToDo
        // Eventually test that fixrec triggers a rerun in the engine properly

        dropOff = new();
        Sut.AnalyzeRecord(param);
        dropOff.Reports.ShouldBeEmpty();
    }

    public void RunShouldBeNoError(
        Action<TMajor> prep)
    {
        var rec = _fixture.Create<TMajor>();
        prep(rec);
        var dropOff = new TestDropoff();
        var param = new IsolatedRecordAnalyzerParams<TMajorGetter>(
            mod: null!,
            record: rec,
            parameters: default,
            reportDropbox: dropOff);
        Sut.AnalyzeRecord(param);
        dropOff.Reports.ShouldBeEmpty();
    }
}
