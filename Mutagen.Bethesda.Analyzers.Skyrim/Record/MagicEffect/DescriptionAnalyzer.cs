using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.MagicEffect;

public class DescriptionAnalyzer : IIsolatedRecordAnalyzer<IMagicEffectGetter>
{
     public static readonly TopicDefinition MagicEffectDescriptionList = MutagenTopicBuilder.FromDiscussion(
             547,
             "Single '%' in Magic Effect Description",
             Severity.Suggestion)
         .WithoutFormatting("% in Magic Effect description should be followed by a second %");

    public IEnumerable<TopicDefinition> Topics { get; } = [MagicEffectDescriptionList];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IMagicEffectGetter> param)
    {
        var mgef = param.Record;
        if (mgef.Description is null) return;
        foreach (Language languages in Enum.GetValues(typeof(Language)))
        {
            string? desc = mgef.Description.Lookup(languages);
            if ((desc is null)) continue;
            int i = 0;
            while (i < desc.Length)
            {
                if (desc[i] == '%')
                {
                    if (i+1 >= desc.Length || desc[i + 1] != '%')
                    {
                        param.AddTopic(MagicEffectDescriptionList.Format());
                        break;
                    }
                    i += 2;
                }
            }
        }
    }
    public IEnumerable<Func<IMagicEffectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Description;
    }

}
