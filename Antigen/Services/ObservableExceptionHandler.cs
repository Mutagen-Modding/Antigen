using System.Diagnostics;
using System.Reactive.Concurrency;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Antigen.Services;

public sealed class ObservableExceptionHandler(ILogger<ObservableExceptionHandler> logger) : IObserver<Exception>
{
    public void OnNext(Exception value)
    {
        if (Debugger.IsAttached) Debugger.Break();

        logger.LogError("Error occured: {Message}", value.ToString());

        RxSchedulers.MainThreadScheduler.Schedule(() => throw value);
    }

    public void OnError(Exception error)
    {
        if (Debugger.IsAttached) Debugger.Break();

        RxSchedulers.MainThreadScheduler.Schedule(() => throw error);
    }

    public void OnCompleted()
    {
        if (Debugger.IsAttached) Debugger.Break();

        RxSchedulers.MainThreadScheduler.Schedule(() => throw new InvalidOperationException("Observable completed unexpectedly."));
    }
}
