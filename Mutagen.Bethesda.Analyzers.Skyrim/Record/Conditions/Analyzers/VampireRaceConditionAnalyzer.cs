using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;

using Races = FormKeys.SkyrimSE.Skyrim.Race;

public class VampireRaceConditionAnalyzer : IConditionAnalyzer
{
    public static readonly TopicDefinition<IConditionGetter, IFormLinkGetter<IRaceGetter>> NoVampireRace = MutagenTopicBuilder.FromDiscussion(
            602,
            "No vampire condition",
            Severity.Warning)
        .WithFormatting<IConditionGetter, IFormLinkGetter<IRaceGetter>>("Condition {0} checks for mortal race {1} but not its vampire equivalent");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoVampireRace];

    private static readonly Dictionary<IFormLinkGetter<IRaceGetter>, IFormLinkGetter<IRaceGetter>> VampireRaceLookup = new()
    {
        { Races.ArgonianRace, Races.ArgonianRaceVampire },
        { Races.BretonRace, Races.BretonRaceVampire },
        { Races.DarkElfRace, Races.DarkElfRaceVampire },
        { Races.HighElfRace, Races.HighElfRaceVampire },
        { Races.ImperialRace, Races.ImperialRaceVampire },
        { Races.KhajiitRace, Races.KhajiitRaceVampire },
        { Races.NordRace, Races.NordRaceVampire },
        { Races.OrcRace, Races.OrcRaceVampire },
        { Races.RedguardRace, Races.RedguardRaceVampire },
        { Races.WoodElfRace, Races.WoodElfRaceVampire },
    };

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IGetIsRaceConditionDataGetter);
        yield return typeof(IGetPCIsRaceConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        var condition = context.Condition;
        IFormLinkGetter<IRaceGetter> raceLink;
        switch (condition.Data)
        {
            case IGetIsRaceConditionDataGetter getRace:
                raceLink = getRace.Race.Link;
                break;
            case IGetPCIsRaceConditionDataGetter getRace:
                raceLink = getRace.Race.Link;
                break;
            default:
                return;
        }

        if (!VampireRaceLookup.TryGetValue(raceLink, out var vampire)) return;

        if (!CheckVampireCondition(condition, vampire, context.Conditions, context.OrBlock))
            context.Param.AddTopic(NoVampireRace.Format(condition, raceLink));
    }

    private static IFormLinkGetter<IRaceGetter> GetComparisonRace(IConditionDataGetter data)
    {
        return data switch
        {
            IGetIsRaceConditionDataGetter getRace => getRace.Race.Link,
            IGetPCIsRaceConditionDataGetter getRace => getRace.Race.Link,
            _ => FormLink<IRaceGetter>.Null
        };
    }

    private static bool CheckVampireCondition(IConditionGetter condition, IFormLinkGetter<IRaceGetter> vampireRace, IEnumerable<IConditionGetter> allConditions, IEnumerable<IConditionGetter> orBlock)
    {
        if (condition is not IConditionFloatGetter conditionFloat)
            return true;

        bool ChecksVampire(IEnumerable<IConditionGetter> block)
        {
            return block.Any(
                c => c is IConditionFloatGetter cf
                     // Same function
                     && cf.Data.Function == conditionFloat.Data.Function
                     // Runs on same ref
                     && cf.Data.RunOnType == conditionFloat.Data.RunOnType
                     && cf.Data.Reference.Equals(conditionFloat.Data.Reference)
                     && cf.Data.RunOnTypeIndex == conditionFloat.Data.RunOnTypeIndex
                     // Same include/exclude
                     && cf.ComparisonValue == conditionFloat.ComparisonValue
                     // Checks vampire race
                     && GetComparisonRace(c.Data).Equals(vampireRace));
        }

        return conditionFloat.ComparisonValue switch
        {
            // Negative conditions should be combined with AND
            0 => ChecksVampire(allConditions) && !ChecksVampire(orBlock),
            // Positive conditions should be combined with OR
            1 => ChecksVampire(orBlock),
            _ => true
        };
    }
}
