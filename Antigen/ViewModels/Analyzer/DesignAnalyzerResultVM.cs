using Antigen.Models.Analyzer;
using Mutagen.Bethesda.Analyzers;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.ViewModels.Analyzer;

/// <summary>
///     Design-time instance of <see cref="AnalyzerResultVM" /> for XAML designer support.
/// </summary>
public sealed class DesignAnalyzerResultVM() : AnalyzerResultVM(CreateDesignData())
{
    private static AnalyzerResultInfo CreateDesignData()
    {
        return new AnalyzerResultInfo
        {
            Result = new AnalyzerResult
            {
                Topic = Topic.Create(new FormattedTopicDefinition
                    {
                        TopicDefinition = MutagenTopicBuilder.DevelopmentTopic("Test Analyzer Result", Severity.Error)
                            .WithoutFormatting("This is a test analyzer result for design-time support.")
                    },
                    typeof(IAnalyzer),
                    [("Name", "Some Meta Data")]
                ),
                Record = new FormLinkInformation(FormKey.Factory("123456:TestMod.esp"), typeof(IMajorRecordGetter)),
                ModKey = ModKey.FromFileName("TestMod.esp")
            },
            ResultEditorId = "0x001234",
            RecordDisplayName = "Record01",
            ParentDisplayName = "Parent01",
            ParentIdentifier = new MajorRecordIdentifier
            {
                FormKey = FormKey.Factory("654321:TestMod.esp"),
                EditorID = "ParentEditorID"
            }
        };
    }
}