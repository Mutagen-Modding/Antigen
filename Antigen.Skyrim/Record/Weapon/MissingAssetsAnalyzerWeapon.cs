using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Weapon;

public class MissingAssetsAnalyzerWeapon : IIsolatedRecordAnalyzer<IWeaponGetter>
{
    private readonly MissingAssetsAnalyzerUtil _util;

    public static readonly TopicDefinition<string> MissingWeaponModel = MutagenTopicBuilder.FromDiscussion(
            92,
            "Missing Weapon Model file",
            Severity.Error)
        .WithFormatting<string>("Missing Model file {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingWeaponModel];

    public MissingAssetsAnalyzerWeapon(MissingAssetsAnalyzerUtil util)
    {
        _util = util;
    }

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IWeaponGetter> param)
    {
        _util.CheckForMissingModelAsset(param, MissingWeaponModel);
    }

    public IEnumerable<Func<IWeaponGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Model!.File;
    }
}
