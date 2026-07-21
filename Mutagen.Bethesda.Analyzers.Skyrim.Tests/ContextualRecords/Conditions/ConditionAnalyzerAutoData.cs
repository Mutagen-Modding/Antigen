using AutoFixture;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;
using Mutagen.Bethesda.Testing.AutoData;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions;

public class ConditionAnalyzerAutoData : MutagenModAutoDataAttribute
{
    public ConditionAnalyzerAutoData()
    {
        Fixture.Register(() => new ConditionAnalyzer(
            typeof(ConditionAnalyzer).Assembly
                .GetTypes()
                .Where(x => x is { IsAbstract: false, IsInterface: false } && x.IsAssignableTo(typeof(IConditionAnalyzer)))
                .Select(x => (IConditionAnalyzer)Activator.CreateInstance(x)!)));
    }
}
