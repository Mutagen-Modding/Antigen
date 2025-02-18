using Autofac;
using FluentAssertions;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.Engines;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Testing;
using Mutagen.Bethesda.Environments;
using NSubstitute;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Integration;

public class RunConfigApplicationTests
{
    public static readonly TopicDefinition Suggestion = new(
        "A123",
        "TestTitle",
        Severity.Suggestion);
    public static readonly IContextualAnalyzer SuggestionAnalyzer;
    public static readonly TopicDefinition Warning = new(
        "A123",
        "TestTitle",
        Severity.Warning);
    public static readonly IContextualAnalyzer WarningAnalyzer;

    static RunConfigApplicationTests()
    {
        SuggestionAnalyzer = new TestAnalyzer(Suggestion);
        WarningAnalyzer = new TestAnalyzer(Warning);
    }

    public class Payload
    {
        private readonly IGameEnvironment _env;

        public Payload(IGameEnvironment env)
        {
            _env = env;
        }

        public async Task<TestDropoff> RunTest(Action<ContainerBuilder> containerAdjustment)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<TestModule>();
            builder.RegisterInstance(new TestGameEnvironmentProvider(_env)).AsImplementedInterfaces();
            builder.RegisterInstance(_env).AsImplementedInterfaces();
            containerAdjustment(builder);
            var container = builder.Build();
            var engine = container.Resolve<ContextualEngine>();
            await engine.Run(CancellationToken.None);
            return container.Resolve<TestDropoff>();
        }
    }

    [Theory, AnalyzerAutoData]
    public async Task NoReturnActsNormally(
        Payload payload,
        IContextualAnalyzer testAnalyzer)
    {
        var dropoff = await payload.RunTest(builder =>
        {
            testAnalyzer.Analyze(default);
            builder.RegisterInstance(testAnalyzer).AsSelf();
        });
        dropoff.Reports.Should().HaveCount(0);
    }

    [Theory, AnalyzerAutoData]
    public async Task PassesDesiredSeverity(Payload payload)
    {
        var dropoff = await payload.RunTest(builder =>
        {
            builder.RegisterInstance(WarningAnalyzer).AsImplementedInterfaces();
        });
        dropoff.Reports.Should().HaveCount(1);
    }

    [Theory, AnalyzerAutoData]
    public async Task FiltersUndesiredSeverity(Payload payload)
    {
        var dropoff = await payload.RunTest(builder =>
        {
            builder.RegisterInstance(SuggestionAnalyzer).AsImplementedInterfaces();
            var minSev = Substitute.For<IMinimumSeverityConfiguration>();
            minSev.MinimumSeverity.Returns(Severity.Warning);
            builder.RegisterInstance(minSev).As<IMinimumSeverityConfiguration>();
        });
        dropoff.Reports.Should().HaveCount(0);
    }

    [Theory, AnalyzerAutoData]
    public async Task AdjustsTopicSeverity(Payload payload)
    {
        var dropoff = await payload.RunTest(builder =>
        {
            builder.RegisterInstance(WarningAnalyzer).AsImplementedInterfaces();
            var minSev = Substitute.For<IMinimumSeverityConfiguration>();
            minSev.MinimumSeverity.Returns(Severity.Warning);
            builder.RegisterInstance(minSev).As<IMinimumSeverityConfiguration>();
            var sevLookup = Substitute.For<ISeverityLookup>();
            sevLookup.LookupSeverity(Warning).Returns(Severity.Suggestion);
            builder.RegisterInstance(sevLookup).As<ISeverityLookup>();
        });
        dropoff.Reports.Should().HaveCount(0);
    }
}
