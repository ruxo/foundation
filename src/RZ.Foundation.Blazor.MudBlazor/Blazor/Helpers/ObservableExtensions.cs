using System.Reactive.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace RZ.Foundation.Blazor.Helpers;

[PublicAPI]
public static class ObservableExtensions
{
    public static IObservable<Outcome<T>> ReportFailure<T>(this IObservable<Outcome<T>> source, ILogger logger, ISnackbar snackbar)
        => source.Do(v => {
            if (v.IfFail(out var e, out _)){
                logger.LogError("Operation failed: {@Error}", e);
                snackbar.Add(e.Message, Severity.Error);
            }
        });
}