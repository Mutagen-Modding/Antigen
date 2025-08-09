using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class QuestExtensions
{
    public static bool HasAlias(this IQuestGetter quest, uint aliasIndex)
    {
        return quest.Aliases.Any(alias => alias.ID == aliasIndex);
    }

    public static IQuestAliasGetter? GetAlias(this IQuestGetter quest, uint aliasIndex)
    {
        return quest.Aliases.FirstOrDefault(alias => alias.ID == aliasIndex);
    }

    public static IQuestAliasGetter? GetAlias(this IQuestGetter quest, string aliasName)
    {
        return quest.Aliases.FirstOrDefault(alias => string.Equals(alias.Name, aliasName, StringComparison.OrdinalIgnoreCase));
    }
}
