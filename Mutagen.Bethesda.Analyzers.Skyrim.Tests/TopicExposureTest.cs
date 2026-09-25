using System.Reflection;
using Autofac;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;
using Mutagen.Bethesda.Analyzers.Testing;
using Mutagen.Bethesda.Plugins.Meta;
using Noggog;
using Shouldly;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests;

public class TopicExposureTest
{
    class TopicExposureTestData<T> : TheoryData<T>
    {
        public TopicExposureTestData()
        {
            foreach (var analyser in GetAllAnalyzers())
            {
                Add(analyser);
            }
        }

        static IEnumerable<T> GetAllAnalyzers()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(GameConstants.Get(GameRelease.SkyrimSE)).As<GameConstants>();
            builder.RegisterModule<SkyrimAnalyzerModule>();
            builder.RegisterModule<TestModule>();
            var container = builder.Build();
            return container.Resolve<IEnumerable<T>>();
        }
    }

    IEnumerable<TopicDefinition> GetDeclaredTopics(Type type)
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField)
            .Select(f => f.GetValue(null) as TopicDefinition)
            .WhereNotNull();
    }


    [Theory, ClassData(typeof(TopicExposureTestData<IAnalyzer>))]
    public void AllTopicsExposed(IAnalyzer analyzer)
    {
        // Special case: ConditionAnalyzer gets its topics from IConditionAnalyzer types
        if (analyzer is ConditionAnalyzer)
            return;

        GetDeclaredTopics(analyzer.GetType()).ShouldBe(analyzer.Topics, ignoreOrder: true, customMessage: analyzer.GetType().FullName);
    }

    [Theory, ClassData(typeof(TopicExposureTestData<IConditionAnalyzer>))]
    public void AllConditionTopicsExposed(IConditionAnalyzer analyzer)
    {
        GetDeclaredTopics(analyzer.GetType()).ShouldBe(analyzer.Topics, ignoreOrder: true, customMessage: analyzer.GetType().FullName);
    }
}
