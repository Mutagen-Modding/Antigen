using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Activator;

public class MineOreAnalyzer : IContextualRecordAnalyzer<IActivatorGetter>
{
    public static readonly TopicDefinition NoMineOreScript = MutagenTopicBuilder.FromDiscussion(
            214,
            "No MineOreScript",
            Severity.Error)
        .WithoutFormatting("Mine Ore does not have a MineOreScript attached");

    public static readonly TopicDefinition NoOreProperty = MutagenTopicBuilder.FromDiscussion(
            298,
            "No Ore Property",
            Severity.Error)
        .WithoutFormatting("Mine ore has no Ore property on MineOreScript");

    public static readonly TopicDefinition<IMiscItemGetter> IncorrectVeinOre = MutagenTopicBuilder.FromDiscussion(
            299,
            "Correct Vein/Ore",
            Severity.Warning)
        .WithFormatting<IMiscItemGetter>("Mine ore uses incorrect ore: {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoMineOreScript, NoOreProperty, IncorrectVeinOre];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IActivatorGetter> param)
    {
        var activator = param.Record;
        if (activator.EditorID is null) return;
        if (!activator.EditorID.Contains("MineOre") && !activator.EditorID.Contains("MineGem")) return;

        // No MineOreScript
        var script = activator.VirtualMachineAdapter?.Scripts.FirstOrDefault(s => string.Equals(s.Name, "MineOreScript", StringComparison.OrdinalIgnoreCase));
        if (script is null)
        {
            param.AddTopic(
                NoMineOreScript.Format());
            return;
        }

        // Incorrect Vein/Ore
        var oreProperty = script.GetProperty<IScriptObjectPropertyGetter>("Ore");
        if (oreProperty is null)
        {
            param.AddTopic(
                NoOreProperty.Format());
            return;
        }

        var oreLinks = oreProperty.EnumerateFormLinks().ToList();
        if (oreLinks.Count == 0)
        {
            param.AddTopic(
                NoOreProperty.Format());
            return;
        }

        var ore = oreLinks[0].TryResolve<IMiscItemGetter>(param.LinkCache);
        if (ore is null)
        {
            param.AddTopic(
                NoOreProperty.Format());
            return;
        }

        var oreSubStrings = ore.Name?.String?.Split(' ');
        if (oreSubStrings is null)
        {
            param.AddTopic(
                NoOreProperty.Format());
            return;
        }

        foreach (var subString in oreSubStrings)
        {
            if (subString.Equals("Ore", StringComparison.Ordinal) || subString.Equals("Gem", StringComparison.OrdinalIgnoreCase)) continue;

            // When part of the ore name is found in the activator's editor ID, we assume it's the correct ore
            if (activator.EditorID.Contains(subString, StringComparison.OrdinalIgnoreCase)) return;
        }

        param.AddTopic(
            IncorrectVeinOre.Format(ore));
    }
    public IEnumerable<Func<IActivatorGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.VirtualMachineAdapter!.Scripts;
    }
}
