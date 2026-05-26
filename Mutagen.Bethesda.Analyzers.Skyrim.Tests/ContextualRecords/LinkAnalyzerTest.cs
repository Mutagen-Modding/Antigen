using Mutagen.Bethesda.Analyzers.Skyrim.Record;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords;

using Fixture = ContextualRecordTestFixture<LinkAnalyzer, Book, ISkyrimMajorRecordGetter>;

public class LinkAnalyzerTest
{
    static readonly FormKey InvalidKey = FormKey.Factory("123456:DoesNotExist.esp");

    // A link must resolve to a record
    [Theory, MutagenModAutoData]
    public void InvalidLink(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.InventoryArt.SetTo(InvalidKey);
            },
            prepForFix: (rec, mod) =>
            {
                var stat = fixture.Create<Static>();
                mod.Statics.Add(stat);
                rec.InventoryArt.SetTo(stat);
            },
            LinkAnalyzer.InvalidLink);
    }

    // A link may be null
    [Theory, MutagenModAutoData]
    public void NullLink(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.InventoryArt.SetTo(InvalidKey);
            },
            prepForFix: (rec, mod) =>
            {
                rec.InventoryArt.SetToNull();
            },
            LinkAnalyzer.InvalidLink);
    }

    // A link must resolve to the correct type
    [Theory, MutagenModAutoData]
    public void WrongType(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var wrongType = fixture.Create<Npc>();
                mod.Npcs.Add(wrongType);
                rec.InventoryArt.SetTo(wrongType.FormKey);
            },
            prepForFix: (rec, mod) =>
            {
                rec.InventoryArt.SetToNull();
            },
            LinkAnalyzer.InvalidLink);
    }

    // A link may resolve to a hardcoded record not present in Skyrim.esm
    [Theory, MutagenModAutoData]
    public void Hardcoded(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.Model = new()
                {
                    AlternateTextures = [new() { NewTexture = InvalidKey.ToLink<ITextureSetGetter>() }]
                };
            },
            prepForFix: (rec, mod) =>
            {
                rec.Model!.AlternateTextures![0].NewTexture.SetTo(FormKeys.SkyrimSE.Skyrim.TextureSet.NullTextureSet);
            },
            LinkAnalyzer.InvalidLink);
    }

    // Hardcoded links must be of the correct type
    [Theory, MutagenModAutoData]
    public void HardcodedWrongType(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.InventoryArt.SetTo(FormKeys.SkyrimSE.Skyrim.TextureSet.NullTextureSet.FormKey);
            },
            prepForFix: (rec, mod) =>
            {
                rec.InventoryArt.SetToNull();
            },
            LinkAnalyzer.InvalidLink);
    }
}
