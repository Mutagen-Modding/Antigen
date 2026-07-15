using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Noggog;
using ReactiveUI;

namespace Antigen.ViewModels;

public abstract class ViewModel : ReactiveObject, IActivatableViewModel, IDisposableDropoff
{
    private readonly Lazy<CompositeDisposable> _compositeDisposable = new();
    protected readonly IDisposableBucket ActivatedDisposable = new DisposableBucket();

    protected ViewModel()
    {
        this.WhenActivated(disposable =>
        {
            Disposable
                .Create(() => ActivatedDisposable.Clear())
                .DisposeWith(disposable);

            WhenActivated();
        });
    }
    public ViewModelActivator Activator { get; } = new();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Add(IDisposable disposable)
    {
        _compositeDisposable.Value.Add(disposable);
    }

    protected virtual void WhenActivated() {}

    protected void RaiseAndSetIfChanged<T>(ref T item, T newItem, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(item, newItem)) return;

        item = newItem;
        this.RaisePropertyChanged(propertyName);
    }

    public override string ToString()
    {
        return GetType().Name;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        ActivatedDisposable.Dispose();
        if (_compositeDisposable.IsValueCreated)
        {
            _compositeDisposable.Value.Dispose();
        }
    }
}
