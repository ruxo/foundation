using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using RZ.Foundation.Types;

namespace RZ.Foundation.Blazor;

[PublicAPI]
public static class Prelude
{
    public static IObservable<T> Observe<T>(Func<Outcome<T>> func) {
        return Observable.Create<T>(OnSubscribe);

        IDisposable OnSubscribe(IObserver<T> observer) {
            var result = func();
            if (result.IfSuccess(out var v, out var e)){
                observer.OnNext(v);
                observer.OnCompleted();
            }
            else
                observer.OnError(new ErrorInfoException(e));
            return Disposable.Empty;
        }
    }

    public static IObservable<T> Observe<T>(Func<CancellationToken, ValueTask<Outcome<T>>> func) =>
        Observable.Create<T>(async (observer, token) => {
            var result = await func(token);
            if (result.IfSuccess(out var v, out var e)){
                observer.OnNext(v);
                observer.OnCompleted();
            }
            else
                observer.OnError(new ErrorInfoException(e));
        });

    public static IObservable<T> Observe<T>(Func<CancellationToken, Outcome<IAsyncEnumerable<T>>> func) =>
        Observable.Create<T>(async (observer, token) => {
            var result = func(token);
            if (result.IfSuccess(out var enumerable, out var e))
                try{
                    await foreach (var v in enumerable.WithCancellation(token))
                        observer.OnNext(v);
                    observer.OnCompleted();
                }
                catch (Exception ex){
                    observer.OnError(ex);
                }
            else
                observer.OnError(new ErrorInfoException(e));
        });
}