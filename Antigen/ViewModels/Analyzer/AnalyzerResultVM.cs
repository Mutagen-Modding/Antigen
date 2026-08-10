using System.Reactive.Subjects;
using Antigen.Models.Analyzer;
using Antigen.Models.Settings;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Plugins.Records;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels.Analyzer;

public partial class AnalyzerResultVM : ViewModel
{
    private readonly Action<AnalyzerResultInfo, IgnoreType> _ignore;
    private readonly Subject<AnalyzerResultVM> _configureRequested = new();

    public AnalyzerResultInfo Info { get; }

    public AnalyzerResult Result => Info.Result;
    public string? ResultEditorId => Info.ResultEditorId;
    public string? RecordDisplayName => Info.RecordDisplayName;
    public string? ParentDisplayName => Info.ParentDisplayName;
    public IMajorRecordIdentifierGetter? ParentIdentifier => Info.ParentIdentifier;

    /// <summary>
    ///     Whether the inline ignore overlay is covering this row.
    /// </summary>
    [Reactive] public partial bool IsConfiguring { get; set; }

    public IObservable<AnalyzerResultVM> ConfigureRequested => _configureRequested;

    public AnalyzerResultVM(AnalyzerResultInfo info, Action<AnalyzerResultInfo, IgnoreType> ignore)
    {
        Info = info;
        _ignore = ignore;
    }

    public string GetIdentifier()
    {
        return Info.GetIdentifier();
    }

    [ReactiveCommand]
    private void RequestConfigure()
    {
        IsConfiguring = !IsConfiguring;
        if (IsConfiguring)
        {
            _configureRequested.OnNext(this);
        }
    }

    [ReactiveCommand]
    private void IgnoreInstance()
    {
        _ignore(Info, IgnoreType.Instance);
        IsConfiguring = false;
    }

    [ReactiveCommand]
    private void IgnoreTopic()
    {
        _ignore(Info, IgnoreType.Topic);
        IsConfiguring = false;
    }

    [ReactiveCommand]
    private void IgnoreRecord()
    {
        _ignore(Info, IgnoreType.Record);
        IsConfiguring = false;
    }

    [ReactiveCommand]
    private void CancelConfigure()
    {
        IsConfiguring = false;
    }
}
