using AutoFixture;
using Mutagen.Bethesda.Analyzers.Drivers;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Caches;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Noggog.Testing.Extensions;
using Shouldly;

namespace Mutagen.Bethesda.Analyzers.Testing.Frameworks;

public class ContextualRecordTestFixture<TAnalyzer, TMajor, TMajorGetter>
    where TMajor : IMajorRecord, TMajorGetter
    where TMajorGetter : IMajorRecordGetter
    where TAnalyzer : IContextualRecordAnalyzer<TMajorGetter>
{
    private readonly IFixture _fixture;
    public TAnalyzer Sut { get; }

    public ContextualRecordTestFixture(TAnalyzer sut, IFixture fixture)
    {
        _fixture = fixture;
        Sut = sut;
    }

    readonly struct TestParameters
    {
        public ISkyrimMod Mod { get; init; }
        public TMajor Rec { get; init; }
        public ILoadOrder<IModListing<ISkyrimMod>> LoadOrder { get; init; }
        public ILinkCache LinkCache { get; init; }
        public TestDropoff DropOff { get; init; }
    };

    public T Create<T>() where T : IMajorRecord => _fixture.Create<T>();

    ContextualRecordAnalyzerParams<TMajorGetter> CreateAnalyserParams(TestParameters baseParams)
    {
        return new ContextualRecordAnalyzerParams<TMajorGetter>(
            linkCache: baseParams.LinkCache,
            loadOrder: baseParams.LoadOrder,
            modKey: ModKey.Null,
            record: baseParams.Rec,
            reportDropbox: baseParams.DropOff,
            // Usage caches are always immutable, therefore we need a new cache after prepForFix
            provideCaches: new ProvideCaches(baseParams.LinkCache, [new UsageCacheProvider(), new ExteriorCellCacheProvider()]));
    }

    TestParameters Setup()
    {
        var mod = new SkyrimMod("mod1.esp", SkyrimRelease.SkyrimSE);
        // TODO: Insert record into mod. GetTopLevelGroup returns a getter
        var rec = _fixture.Create<TMajor>();
        var loadOrder = new LoadOrder<IModListing<ISkyrimMod>>
        {
            new ModListing<ISkyrimMod>(mod)
        };
        var linkCache = mod.ToMutableLinkCache();
        var dropOff = new TestDropoff();
        return new TestParameters()
        {
            Mod = mod,
            Rec = rec,
            LoadOrder = loadOrder,
            LinkCache = linkCache,
            DropOff = dropOff
        };
    }

    public void Run(
        Action<TMajor, ISkyrimMod> prepForError,
        Action<TMajor, ISkyrimMod> prepForFix,
        params TopicDefinition[] expectedTopics)
    {
        var param = Setup();

        prepForError(param.Rec, param.Mod);

        Sut.AnalyzeRecord(CreateAnalyserParams(param));
        param.DropOff.Reports.Select(x => x.TopicDefinition.Id)
            .ShouldEqualEnumerable(expectedTopics.Select(x => x.Id));

        prepForFix(param.Rec, param.Mod);

        // ToDo
        // Eventually test that fixrec triggers a rerun in the engine properly

        param.DropOff.ClearReports();
        Sut.AnalyzeRecord(CreateAnalyserParams(param));
        param.DropOff.Reports.ShouldBeEmpty();
    }

    public void RunShouldBeNoError(
        Action<TMajor, ISkyrimMod> prep)
    {
        var param = Setup();
        prep(param.Rec, param.Mod);
        Sut.AnalyzeRecord(CreateAnalyserParams(param));
        param.DropOff.Reports.ShouldBeEmpty();
    }
}
