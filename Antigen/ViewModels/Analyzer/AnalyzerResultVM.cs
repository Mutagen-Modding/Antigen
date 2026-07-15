using System.Reactive.Subjects;
using Antigen.Models.Analyzer;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Plugins.Records;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels.Analyzer;

public partial class AnalyzerResultVM(AnalyzerResultInfo info) : ViewModel
{
    private readonly Subject<AnalyzerResultVM> _configureRequested = new();

    public AnalyzerResultInfo Info { get; } = info;

    public AnalyzerResult Result => Info.Result;
    public string? ResultEditorId => Info.ResultEditorId;
    public string? RecordDisplayName => Info.RecordDisplayName;
    public string? ParentDisplayName => Info.ParentDisplayName;
    public IMajorRecordIdentifierGetter? ParentIdentifier => Info.ParentIdentifier;

    public IObservable<AnalyzerResultVM> ConfigureRequested => _configureRequested;

    public string GetIdentifier()
    {
        return Info.GetIdentifier();
    }

    [ReactiveCommand]
    private void RequestConfigure()
    {
        _configureRequested.OnNext(this);
    }
}