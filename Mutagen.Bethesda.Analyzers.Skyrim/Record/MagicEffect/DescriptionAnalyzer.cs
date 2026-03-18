using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.MagicEffect;

public class DescriptionAnalyzer : IIsolatedRecordAnalyzer<IMagicEffectGetter>
{
    //TBD
     public static readonly TopicDefinition MagicEffectDescriptionList = MutagenTopicBuilder.FromDiscussion(
             547,
             "MagicEffectDescription",
             Severity.Suggestion)
         .WithoutFormatting("% in Magic Effect description should be followed by a second %");

    public IEnumerable<TopicDefinition> Topics { get; } = [MagicEffectDescriptionList];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IMagicEffectGetter> param)
    {
        var mgef = param.Record;
        if (mgef.Description != null)
        {
            string? desc = mgef.Description.String;
            int i = 0;
            if (desc != null)
            {
                while (i < desc.Length)
                {
                    if (desc[i] == '%')
                    {
                        if (desc[i + 1] != '%')
                        {
                            param.AddTopic(MagicEffectDescriptionList.Format());
                        }
                        else
                        {
                            i++;
                        }
                    }
                    i++;
                }
            }
        }

    }
    public IEnumerable<Func<IMagicEffectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Description;
    }

}
