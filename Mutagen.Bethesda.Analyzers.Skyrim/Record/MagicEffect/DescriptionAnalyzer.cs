using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.MagicEffect;

public class DescriptionAnalyzer : IIsolatedRecordAnalyzer<IMagicEffectGetter>
{
     public static readonly TopicDefinition<Language> MagicEffectDescriptionList = MutagenTopicBuilder.FromDiscussion(
             547,
             "Incorrect usage of '%' in Magic Effect Description",
             Severity.Suggestion)
         .WithFormatting<Language>("MagicEffect Description in {0} contains incorrect % usage. Always use exactly 2 consecutive %");

    public IEnumerable<TopicDefinition> Topics { get; } = [MagicEffectDescriptionList];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IMagicEffectGetter> param)
    {
        var mgef = param.Record;
        if (mgef.Description is null) return;

        foreach (var(language, desc) in mgef.Description)
        {
            int i = 0;
            while (i < desc.Length)
            {
                if (desc[i] == '%')
                {
                    bool nextCharIsPercentageOrEmpty = (i + 1 < desc.Length) && (desc[i + 1] == '%');
                    bool prevCharIsPercentageOrEmpty = (i > 0) && (desc[i - 1] == '%');
                    if (!(nextCharIsPercentageOrEmpty ^ prevCharIsPercentageOrEmpty))
                    {
                        param.AddTopic(MagicEffectDescriptionList.Format(language));
                        break;
                    }
                }
                i++;
            }
        }
    }
    public IEnumerable<Func<IMagicEffectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Description;
    }

}
