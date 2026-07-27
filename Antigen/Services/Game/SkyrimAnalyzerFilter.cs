using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.Skyrim.Record;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.ArmorAddon;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.HeadPart;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Static;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.TextureSet;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Weapon;

namespace Antigen.Services.Game;

public sealed class SkyrimAnalyzerFilter : IAnalyzerFilter
{
    public bool ShouldAnalyze(IAnalyzer analyzer)
    {
        return analyzer is not LinkAnalyzer
            and not MissingAssetsAnalyzerArmor
            and not MissingAssetsAnalyzerArmorAddon
            and not MissingAssetsAnalyzerHeadPart
            and not MissingAssetsAnalyzerStatic
            and not MissingAssetsAnalyzerTextureSet
            and not MissingAssetsAnalyzerWeapon;
    }
}