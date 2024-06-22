using System.Reactive;
using System.Reactive.Linq;
using RZ.Foundation.Types;
using Unit = LanguageExt.Unit;

namespace RZ.Foundation.Blazor.Helpers;

public static class ObservableExtensions
{
    public static IObservable<ErrorInfo> GetErrorStream<T>(this IObservable<T> stream) =>
        from r in stream.Materialize()
        where r.Kind == NotificationKind.OnError
        let e = r.Exception!
        select e is ErrorInfoException ei
                   ? new ErrorInfo(ei.Code, ei.Message)
                   : new ErrorInfo(StandardErrorCodes.Unhandled, e.ToString());

    public static IObservable<Unit> Ignore<_>(this IObservable<_> stream) =>
        stream.Select(_ => LanguageExt.Prelude.unit);
}