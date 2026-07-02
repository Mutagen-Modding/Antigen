using AutoFixture;
using Mutagen.Bethesda;
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
    where TMajorGetter : class, IMajorRecordGetter
    where TAnalyzer : IContextualRecordAnalyzer<TMajorGetter>
{
    private readonly IFixture _fixture;
    private readonly SkyrimMod _mod;
    private readonly TMajor _rec;
    public TAnalyzer Sut { get; }

    public ContextualRecordTestFixture(TAnalyzer sut, IFixture fixture, SkyrimMod mod, TMajor rec)
    {
        _fixture = fixture;
        Sut = sut;
        _mod = mod;
        _rec = rec;
    }

    readonly struct TestParameters
    {
        public ILoadOrder<IModListing<ISkyrimMod>> LoadOrder { get; init; }
        public ILinkCache LinkCache { get; init; }
        public TestDropoff DropOff { get; init; }
    };

    [Obsolete("Use test case parameters or Group.AddNew instead")]
    public T Create<T>() where T : IMajorRecord {
        var rec = _fixture.Create<T>();
        _mod.Remove(rec); // Fixture.Create inserts the record into our mod which existing test cases do manually
        return rec;
    }

    ContextualRecordAnalyzerParams<TMajorGetter> CreateAnalyserParams(TestParameters baseParams)
    {
        return CreateAnalyserParams(_rec, baseParams.LinkCache, baseParams.LoadOrder, baseParams.DropOff);
    }

    ContextualRecordAnalyzerParams<TMajorGetter> CreateAnalyserParams(
        TMajorGetter record,
        ILinkCache linkCache,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder,
        TestDropoff dropOff)
    {
        return new ContextualRecordAnalyzerParams<TMajorGetter>(
            linkCache: linkCache,
            loadOrder: loadOrder,
            modKey: _mod.ModKey,
            record: record,
            reportDropbox: dropOff,
            // Caches are immutable, so a fresh ProvideCaches is needed after prepForFix
            provideCaches: new ProvideCaches(linkCache, TestCacheConstructors.All));
    }

    TestParameters Setup()
    {
        var loadOrder = new LoadOrder<IModListing<ISkyrimMod>>
        {
            new ModListing<ISkyrimMod>(_mod)
        };
        var linkCache = _mod.ToMutableLinkCache();
        var dropOff = new TestDropoff();
        return new TestParameters()
        {
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

        prepForError(_rec, _mod);

        Sut.AnalyzeRecord(CreateAnalyserParams(param));
        param.DropOff.Reports.Select(x => x.TopicDefinition.Id)
            .ShouldEqualEnumerable(expectedTopics.Select(x => x.Id));

        prepForFix(_rec, _mod);

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
        prep(_rec, _mod);
        Sut.AnalyzeRecord(CreateAnalyserParams(param));
        param.DropOff.Reports.ShouldBeEmpty();
    }

    readonly struct FileParameters
    {
        public IModGetter Mod { get; init; }
        public ILoadOrderGetter<IModListingGetter<IModGetter>> LoadOrder { get; init; }
        public ILinkCache LinkCache { get; init; }
    }

    FileParameters SetupFromFile(ModPath path, GameRelease release)
    {
        var mod = ModFactory.ImportGetter(path, release);
        var loadOrder = new LoadOrder<IModListingGetter<IModGetter>>
        {
            new ModListing<IModGetter>(mod, enabled: true)
        };
        return new FileParameters
        {
            Mod = mod,
            LoadOrder = loadOrder,
            LinkCache = mod.ToUntypedImmutableLinkCache(),
        };
    }

    void AnalyzeFromFile(
        FileParameters param,
        FormKey record,
        params TopicDefinition[] expectedTopics)
    {
        var dropOff = new TestDropoff();
        var rec = param.LinkCache.Resolve<TMajorGetter>(record);
        Sut.AnalyzeRecord(CreateAnalyserParams(rec, param.LinkCache, param.LoadOrder, dropOff));
        dropOff.Reports.Select(x => x.TopicDefinition.Id)
            .ShouldEqualEnumerable(expectedTopics.Select(x => x.Id));
    }

    public void RunWithFile(
        ModPath path,
        GameRelease release,
        FormKey errorRecord,
        FormKey fixRecord,
        params TopicDefinition[] expectedTopics)
    {
        var param = SetupFromFile(path, release);
        using var mod = param.Mod as IDisposable;
        AnalyzeFromFile(param, errorRecord, expectedTopics);
        AnalyzeFromFile(param, fixRecord);
    }

    public void RunWithFile(
        ModPath path,
        GameRelease release,
        FormKey record,
        params TopicDefinition[] expectedTopics)
    {
        var param = SetupFromFile(path, release);
        using var mod = param.Mod as IDisposable;
        AnalyzeFromFile(param, record, expectedTopics);
    }
}
