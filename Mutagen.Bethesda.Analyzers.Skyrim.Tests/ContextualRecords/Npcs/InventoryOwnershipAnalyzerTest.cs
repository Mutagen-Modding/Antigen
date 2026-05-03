using Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Noggog;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Npcs;

using Fixture = ContextualRecordTestFixture<InventoryOwnershipAnalyzer, Npc, INpcGetter>;

public class InventoryOwnershipAnalyzerTests
{
    [Theory, MutagenModAutoData]
    public void DetectsNpcOwningOwnInventoryItem(Fixture fixture)
    {
        var item = fixture.Create<Weapon>();

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                mod.Weapons.Add(item);

                var entry = new ContainerEntry();
                entry.Item.Item.SetTo(item);

                var owner = new NpcOwner();
                owner.Npc.SetTo(rec);
                entry.Data = new ExtraData();
                entry.Data.Owner = owner;

                rec.Items ??= new ExtendedList<ContainerEntry>();
                rec.Items.Add(entry);
            },
            prepForFix: (rec, mod) =>
            {
                rec.Items!.Clear();
            },
            InventoryOwnershipAnalyzer.InventoryItemWithOwner
        );
    }
}
