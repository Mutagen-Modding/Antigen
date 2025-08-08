using System.Text.RegularExpressions;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Util;

public static partial class TextReplacementUtil
{
    [GeneratedRegex(@"<(\w+)(?:.(\w+))?=(\w+)>", RegexOptions.IgnoreCase)]
    public static partial Regex TextReplacementRegex { get; }

    public static void CheckString(
        IQuestGetter quest,
        ILinkCache linkCache,
        ITranslatedStringGetter translatedString,
        Action<Language, string> hasGlobalVariable,
        Action<Language, string> missingGlobalVariable,
        Action<Language, string> missingAlias,
        Action<Language, string> aliasWithoutFlag)
    {
        foreach (var (language, text) in translatedString)
        {
            foreach (Match match in TextReplacementRegex.Matches(text))
            {
                var value = match.Groups[1].Value;
                switch (value)
                {
                    case "Global":
                        var globalEditorId = match.Groups[3].Value;
                        if (linkCache.TryResolveIdentifier<IGlobalGetter>(globalEditorId, out var global))
                        {
                            if (quest.TextDisplayGlobals.All(x => x.FormKey != global))
                            {
                                hasGlobalVariable(language, globalEditorId);
                            }
                        }
                        else
                        {
                            missingGlobalVariable(language, globalEditorId);
                        }
                        break;
                    case "Alias":
                        CheckAlias(match.Groups[3].Value);
                        break;
                    case "Relationship":
                        CheckAlias(match.Groups[2].Value);
                        CheckAlias(match.Groups[3].Value);
                        break;
                }
            }

            void CheckAlias(string aliasName)
            {
                // Player can be referenced without there being an alias named Player
                if (aliasName.Equals("Player", StringComparison.OrdinalIgnoreCase)) return;

                var alias = quest.Aliases.FirstOrDefault(a => string.Equals(a.Name, aliasName, StringComparison.OrdinalIgnoreCase));
                if (alias is null)
                {
                    missingAlias(language, aliasName);
                }
                else if (!alias.Flags.HasValue || !alias.Flags.Value.HasFlag(QuestAlias.Flag.StoresText))
                {
                    aliasWithoutFlag(language, aliasName);
                }
            }
        }
    }
}
