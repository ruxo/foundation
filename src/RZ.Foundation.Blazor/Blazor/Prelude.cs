using System.Reactive.Disposables;
using System.Reactive.Linq;
using RZ.Foundation.Functional;
using RZ.Foundation.Types;

namespace RZ.Foundation.Blazor;

public static class Prelude
{
    public static IObservable<T> Observe<T>(Func<OutcomeT<Synchronous, T>> func) {
        return Observable.Create<T>(OnSubscribe);

        IDisposable OnSubscribe(IObserver<T> observer) {
            var result = func().RunIO();
            if (result.IfSuccess(out var v, out var e)){
                observer.OnNext(v);
                observer.OnCompleted();
            }
            else
                observer.OnError(new ErrorInfoException(e));
            return Disposable.Empty;
        }
    }

    public static IObservable<T> Observe<T>(Func<CancellationToken, OutcomeT<Asynchronous, T>> func) =>
        Observable.Create<T>(async (observer, token) => {
            var result = await func(token).RunIO();
            if (result.IfSuccess(out var v, out var e)){
                observer.OnNext(v);
                observer.OnCompleted();
            }
            else
                observer.OnError(new ErrorInfoException(e));
        });

    public static IObservable<T> Observe<T>(Func<CancellationToken, OutcomeT<Asynchronous, IAsyncEnumerable<T>>> func) =>
        Observable.Create<T>(async (observer, token) => {
            var result = await func(token).RunIO();
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