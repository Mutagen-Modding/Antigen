using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Extensions;

public static class LeveledNpcExtensions
{
    extension(ILeveledNpcGetter leveledNpc)
    {
        /// <summary>
        /// Sample npcs from the leveled list based on the provided level.
        /// </summary>
        /// <param name="linkCache">The link cache used to resolve form links.</param>
        /// <param name="level">The level at which to sample npcs from the leveled list.</param>
        /// <param name="random">A random number generator used for sampling.</param>
        /// <returns>An npc if a valid npc is found, otherwise null.</returns>
        public INpcGetter? SampleNpc(ILinkCache linkCache, int level, Random random)
        {
            if (leveledNpc.Entries is null) return null;

            // If checked, the level list will use any Npc at or below the player's level in the list.
            // If not checked, it uses the closest Npc or npcs to the player's level, but not exceeding it.
            var calculateFromAllLevelsLessThanOrEqualPlayer = leveledNpc.Flags.HasFlag(LeveledNpc.Flag.CalculateFromAllLevelsLessThanOrEqualPlayer);

            if (calculateFromAllLevelsLessThanOrEqualPlayer)
            {

                var npcPool = leveledNpc.Entries
                    .Select(e => e.Data)
                    .WhereNotNull()
                    .Where(e => e.Level <= level)
                    .Select(e => e.Reference.TryResolve(linkCache, out var npc) ? npc : null)
                    .WhereNotNull()
                    .Select(n => n switch
                    {
                        ILeveledNpcGetter nestedLeveledNpc => nestedLeveledNpc.SampleNpc(linkCache, level, random),
                        INpcGetter npc => npc,
                        _ => null
                    })
                    .WhereNotNull()
                    .ToArray();

                if (npcPool.Length > 0)
                {
                    var randomIndex = random.Next(0, npcPool.Length);
                    return npcPool[randomIndex];
                }
            }
            else
            {
                // Find the closest level to the player's level without exceeding it.
                var closestNpc = leveledNpc.Entries
                    .Where(e => e.Data is not null && e.Data.Level <= level)
                    .OrderByDescending(e => e.Data!.Level)
                    .Select(e => e.Data!.Reference.TryResolve(linkCache, out var npc) ? npc : null)
                    .WhereNotNull()
                    .Select(n => n switch
                    {
                        ILeveledNpcGetter nestedLeveledNpc => nestedLeveledNpc.SampleNpc(linkCache, level, random),
                        INpcGetter npc => npc,
                        _ => null
                    })
                    .FirstOrDefault();

                if (closestNpc is not null)
                {
                    return closestNpc;
                }
            }

            return null;
        }

        /// <summary>
        /// Sample npcs from the leveled list based on the provided level.
        /// </summary>
        /// <param name="linkCache">The link cache used to resolve form links.</param>
        /// <param name="level">The level at which to sample npcs from the leveled list.</param>
        /// <returns>An npc if a valid npc is found, otherwise null.</returns>
        public long GetAverageGoldValue(ILinkCache linkCache, int level)
        {
            if (leveledNpc.Entries is null) return 0;

            // If a global is used for chance none
            if (!leveledNpc.Global.IsNull) return 0;

            // Use the chance none value to reduce the average gold value of the leveled list.
            var fraction = 1 - leveledNpc.ChanceNone.Value;

            // If checked, the level list will use any Npc at or below the player's level in the list.
            // If not checked, it uses the closest Npc or npcs to the player's level, but not exceeding it.
            var calculateFromAllLevelsLessThanOrEqualPlayer = leveledNpc.Flags.HasFlag(LeveledNpc.Flag.CalculateFromAllLevelsLessThanOrEqualPlayer);

            if (calculateFromAllLevelsLessThanOrEqualPlayer)
            {
                var npcPool = leveledNpc.Entries
                    .Select(e => e.Data)
                    .WhereNotNull()
                    .Where(e => e.Level <= level)
                    .Select(e => e.Reference.TryResolve(linkCache, out var npc) ? npc : null)
                    .WhereNotNull()
                    .Select(n => n switch
                    {
                        ILeveledNpcGetter nestedLeveledNpc => nestedLeveledNpc.GetAverageGoldValue(linkCache, level),
                        INpcGetter npc => npc.GetAverageGoldValue(linkCache, level),
                        _ => 0
                    })
                    .ToArray();

                if (npcPool.Length > 0)
                {
                    return (long) (fraction * npcPool[0]);
                }
            }
            else
            {
                if (leveledNpc.Entries.Count == 0) return 0;

                // Find the closest level to the player's level without exceeding it.
                return (long) (fraction * leveledNpc.Entries
                    .Where(e => e.Data is not null && e.Data.Level <= level)
                    .OrderByDescending(e => e.Data!.Level)
                    .Select(e => e.Data!.Reference.TryResolve(linkCache, out var npc) ? npc : null)
                    .WhereNotNull()
                    .Select(n => n switch
                    {
                        ILeveledNpcGetter nestedLeveledNpc => nestedLeveledNpc.GetAverageGoldValue(linkCache, level),
                        INpcGetter npc => npc.GetAverageGoldValue(linkCache, level),
                        _ => 0
                    })
                    .Average()
                    .Round());
            }

            return 0;
        }
    }
}
