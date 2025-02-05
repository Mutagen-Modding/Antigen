using FluentAssertions;
using Mutagen.Bethesda.Analyzers.Reporting.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Engine.Reporting;

public class EditorIdEnricherTests
{
    public class Payload
    {
        public TestReportDropbox TestReportDropbox { get; }
        public EditorIdEnricher Sut { get; }

        public Payload()
        {
            TestReportDropbox = new TestReportDropbox();
            Sut = new EditorIdEnricher(TestReportDropbox);
        }
    }

    [Theory, MutagenAutoData]
    public void NothingToEnrich(
        ReportContextParameters param,
        Topic topic,
        string str,
        int i,
        IEnumerable<int> ints,
        Dictionary<int, string> dict,
        Payload payload)
    {
        var meta = new (string Name, object Value)[]
        {
            ("String", str),
            ("Int32", i),
            ("Int32s", ints),
            ("Dicts", dict),
        };
        topic = topic with
        {
            MetaData = meta
        };
        payload.Sut.Dropoff(param, topic);
        payload.TestReportDropbox.Dropoffs.Select(x => x.Topics.MetaData)
            .Should().Equal(topic.MetaData);
    }

    [Theory, MutagenModAutoData]
    public void FormLink(
        SkyrimMod mod,
        Npc npc,
        Topic topic,
        string editorId,
        string str,
        Payload payload)
    {
        npc.EditorID = editorId;
        var meta = new (string Name, object Value)[]
        {
            ("FormLink", npc.ToLinkGetter<INpcGetter>()),
        };
        topic = topic with
        {
            MetaData = meta
        };
        payload.Sut.Dropoff(new ReportContextParameters()
        {
            LinkCache = mod.ToImmutableLinkCache()
        }, topic);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .Should().Equal(new MajorRecordIdentifier()
            {
                FormKey = npc.FormKey,
                EditorID = npc.EditorID
            });
    }

    [Theory, MutagenModAutoData]
    public void FormLinkCollection(
        SkyrimMod mod,
        Npc npc,
        Npc npc2,
        Topic topic,
        string editorId,
        string editorId2,
        string str,
        Payload payload)
    {
        npc.EditorID = editorId;
        var meta = new (string Name, object Value)[]
        {
            ("FormLinks", new IFormLinkGetter[]
            {
                npc.ToLinkGetter<INpcGetter>(),
                npc2.ToLinkGetter<INpcGetter>()
            }),
        };
        topic = topic with
        {
            MetaData = meta
        };
        payload.Sut.Dropoff(new ReportContextParameters()
        {
            LinkCache = mod.ToImmutableLinkCache()
        }, topic);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .Should().HaveCount(1);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First().Should().BeOfType<object[]>();
        var arr = payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First() as object[];
        arr.Should().Equal(
            new MajorRecordIdentifier()
            {
                FormKey = npc.FormKey,
                EditorID = npc.EditorID
            },
            new MajorRecordIdentifier()
            {
                FormKey = npc2.FormKey,
                EditorID = npc2.EditorID
            });
    }

    [Theory, MutagenModAutoData]
    public void FormLinkEnumerable(
        SkyrimMod mod,
        Npc npc,
        Npc npc2,
        Topic topic,
        string editorId,
        string editorId2,
        string str,
        Payload payload)
    {
        npc.EditorID = editorId;
        var meta = new (string Name, object Value)[]
        {
            ("FormLinks", npc.ToLinkGetter<INpcGetter>().AsEnumerable()
                .And(npc2.ToLinkGetter<INpcGetter>())),
        };
        topic = topic with
        {
            MetaData = meta
        };
        payload.Sut.Dropoff(new ReportContextParameters()
        {
            LinkCache = mod.ToImmutableLinkCache()
        }, topic);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .Should().HaveCount(1);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First().Should().BeOfType<object[]>();
        var arr = payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First() as object[];
        arr.Should().Equal(
            new MajorRecordIdentifier()
            {
                FormKey = npc.FormKey,
                EditorID = npc.EditorID
            },
            new MajorRecordIdentifier()
            {
                FormKey = npc2.FormKey,
                EditorID = npc2.EditorID
            });
    }

    [Theory, MutagenModAutoData]
    public void FormLinkDictionaryKey(
        SkyrimMod mod,
        Npc npc,
        Npc npc2,
        Topic topic,
        string editorId,
        string editorId2,
        string str,
        Payload payload)
    {
        npc.EditorID = editorId;
        var meta = new (string Name, object Value)[]
        {
            ("FormLinks", new Dictionary<IFormLinkGetter, string>()
            {
                { npc.ToLinkGetter(), "Hello" },
                { npc2.ToLinkGetter(), "World" },
            }),
        };
        topic = topic with
        {
            MetaData = meta
        };
        payload.Sut.Dropoff(new ReportContextParameters()
        {
            LinkCache = mod.ToImmutableLinkCache()
        }, topic);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .Should().HaveCount(1);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First().Should().BeOfType<Dictionary<object, object>>();
        var dict = payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First() as Dictionary<object, object>;
        dict!.Keys.Should().Equal(
            new MajorRecordIdentifier()
            {
                FormKey = npc.FormKey,
                EditorID = npc.EditorID
            },
            new MajorRecordIdentifier()
            {
                FormKey = npc2.FormKey,
                EditorID = npc2.EditorID
            });
        dict!.Values.Should().Equal("Hello", "World");
    }

    [Theory, MutagenModAutoData]
    public void FormLinkDictionaryValue(
        SkyrimMod mod,
        Npc npc,
        Npc npc2,
        Topic topic,
        string editorId,
        string editorId2,
        string str,
        Payload payload)
    {
        npc.EditorID = editorId;
        var meta = new (string Name, object Value)[]
        {
            ("FormLinks", new Dictionary<string, IFormLinkGetter>()
            {
                { "Hello", npc.ToLinkGetter() },
                { "World", npc2.ToLinkGetter() },
            }),
        };
        topic = topic with
        {
            MetaData = meta
        };
        payload.Sut.Dropoff(new ReportContextParameters()
        {
            LinkCache = mod.ToImmutableLinkCache()
        }, topic);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .Should().HaveCount(1);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First().Should().BeOfType<Dictionary<object, object>>();
        var dict = payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First() as Dictionary<object, object>;
        dict!.Keys.Should().Equal("Hello", "World");
        dict!.Values.Should().Equal(
            new MajorRecordIdentifier()
            {
                FormKey = npc.FormKey,
                EditorID = npc.EditorID
            },
            new MajorRecordIdentifier()
            {
                FormKey = npc2.FormKey,
                EditorID = npc2.EditorID
            });
    }

    [Theory, MutagenModAutoData]
    public void FormLinkDictionary(
        SkyrimMod mod,
        Npc npc,
        Npc npc2,
        Topic topic,
        string editorId,
        string editorId2,
        string str,
        Payload payload)
    {
        npc.EditorID = editorId;
        var meta = new (string Name, object Value)[]
        {
            ("FormLinks", new Dictionary<IFormLinkGetter, IFormLinkGetter>()
            {
                { npc.ToLinkGetter(), npc.ToLinkGetter() },
                { npc2.ToLinkGetter(), npc2.ToLinkGetter() },
            }),
        };
        topic = topic with
        {
            MetaData = meta
        };
        payload.Sut.Dropoff(new ReportContextParameters()
        {
            LinkCache = mod.ToImmutableLinkCache()
        }, topic);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .Should().HaveCount(1);
        payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First().Should().BeOfType<Dictionary<object, object>>();
        var dict = payload.TestReportDropbox.Dropoffs.SelectMany(x => x.Topics.MetaData.Select(x => x.Value))
            .First() as Dictionary<object, object>;
        dict!.Keys.Should().Equal(
            new MajorRecordIdentifier()
            {
                FormKey = npc.FormKey,
                EditorID = npc.EditorID
            },
            new MajorRecordIdentifier()
            {
                FormKey = npc2.FormKey,
                EditorID = npc2.EditorID
            });
        dict!.Values.Should().Equal(
            new MajorRecordIdentifier()
            {
                FormKey = npc.FormKey,
                EditorID = npc.EditorID
            },
            new MajorRecordIdentifier()
            {
                FormKey = npc2.FormKey,
                EditorID = npc2.EditorID
            });
    }
}
