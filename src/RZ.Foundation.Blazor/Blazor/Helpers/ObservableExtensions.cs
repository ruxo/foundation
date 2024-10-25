using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RZ.Foundation.Blazor.MVVM;
using RZ.Foundation.Blazor.Shells;
using RZ.Foundation.Types;
using Unit = LanguageExt.Unit;

namespace RZ.Foundation.Blazor.Helpers;

public static class ObservableFrom
{
    [PublicAPI]
    public static IObservable<T> Func<T>(Func<Outcome<T>> func)
        => Observable.Create<T>(observer => observer.Consume(func()));

    [PublicAPI]
    public static IObservable<T> Func<T>(Func<T> func)
        => Observable.Create<T>(observer => {
            try{
                observer.OnNext(func());
                observer.OnCompleted();
            }
            catch (Exception e){
                observer.OnError(e);
            }
            return Disposable.Empty;
        });

    [PublicAPI]
    public static IObservable<T> Generator<T>(Func<IEnumerable<T>> func, bool disposeAfterCall = false)
        => Observable.Create<T>(observer => {
            try{
                var enumerable = func();
                foreach (var v in enumerable)
                    observer.OnNext(v);
                observer.OnCompleted();

                if (disposeAfterCall && enumerable is IDisposable disposable)
                    disposable.Dispose();
            }
            catch (Exception e){
                observer.OnError(e);
            }
            return Disposable.Empty;
        });

    [PublicAPI]
    public static IObservable<T> Outcome<T>(Func<CancellationToken, Task<Outcome<T>>> func)
        => Observable.Create<T>(async (observer, token) => observer.Consume(await func(token)));

    [PublicAPI]
    public static IObservable<T> Outcome<T>(Func<CancellationToken, Task<Outcome<IAsyncEnumerable<T>>>> func)
        => Observable.Create<T>(async (observer, token) => {
            var result = await func(token);
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

[PublicAPI]
public static class ObservableExtensions
{
    public static IDisposable Consume<T>(this IObserver<T> observer, Outcome<T> value) {
        if (value.IfSuccess(out var v, out var e)){
            observer.OnNext(v);
            observer.OnCompleted();
        }
        else
            observer.OnError(new ErrorInfoException(e));
        return Disposable.Empty;
    }

    public static IObservable<Outcome<T>> CatchToOutcome<T>(this IObservable<Outcome<T>> source)
        => source.Catch((Exception e) => Observable.Return(FailedOutcome<T>(e.ToErrorInfo())));

    public static IObservable<Outcome<T>> CatchToOutcome<T>(this IObservable<T> source, Action<ErrorInfo>? handler = default) {
        var stream = source.Select(SuccessOutcome).CatchToOutcome();
        return handler is null
                   ? stream
                   : stream.Do(result => {
                       if (result.IfFail(out var e, out _))
                           handler(e);
                   });
    }

    static ErrorInfo From<T>(Notification<T> r)
        => r.Exception is ErrorInfoException ei
               ? new ErrorInfo(ei.Code, ei.Message)
               : new ErrorInfo(StandardErrorCodes.Unhandled, r.Exception!.ToString());

    [PublicAPI]
    public static IObservable<ErrorInfo> GetErrorStream<T>(this IObservable<T> stream) =>
        from r in stream.Materialize()
        where r.Kind == NotificationKind.OnError
        select From(r);

    public static IObservable<ErrorInfo> GetErrorStream<T>(this IObservable<Outcome<T>> stream) =>
        from r in stream.Materialize()
        let e = r.Kind switch {
            NotificationKind.OnError => From(r),
            NotificationKind.OnNext  => r.Value.IfFail(out var e, out _) ? e : null,
            _                        => null
        }
        where e is not null
        select e;

    public static IObservable<Outcome<T>> SetViewStatus<T>(this IObservable<Outcome<T>> source, Action<ViewStatus> assign)
        => source.Do(result => assign(result.IfSuccess(out _, out var e) ? ViewStatus.Ready.Instance : new ViewStatus.Failed(e)));

    public static IObservable<Unit> Ignore<_>(this IObservable<_> stream)
        => stream.Select(_ => unit);

    /// <summary>
    /// Get the execution handler for a reactive command
    /// </summary>
    public static Func<Task<T>> OnExecute<T>(this ReactiveCommand<Unit, T> command)
        => async () => await command.Execute();

    public static Func<Task<T>> OnExecute<T>(this ReactiveCommand<System.Reactive.Unit, T> command)
        => async () => await command.Execute();

    public static Func<Task<TOut>> OnExecute<TIn,TOut>(this ReactiveCommand<TIn, TOut> command, Func<TIn> value)
        => async () => await command.Execute(value());

    public static IObservable<T> Shared<T>(this IObservable<T> coldObservable)
        => coldObservable.Publish().RefCount();

    static string DefaultTranslator(ErrorInfo e) => e.Message;

    static IDisposable TrapErrors(this IObservable<ErrorInfo> source, ShellViewModel shell,
                                  Func<ErrorInfo, string> translator, ILogger? logger = default) =>
        source.Subscribe(e => {
            if (logger is null)
                Trace.WriteLine($"TrapErrors: {e.Code}:{e.Message}");
            else
                logger.LogError("TrapErrors: {@Error}", e);

            shell.Notify(new(MessageSeverity.Error, translator(e)));
        });

    public static IDisposable TrapErrors<T>(this IObservable<Outcome<T>> source, ShellViewModel shell,
                                            Func<ErrorInfo, string>? translator = default, ILogger? logger = default)
        => source.GetErrorStream().TrapErrors(shell, translator ?? DefaultTranslator, logger);

    public static IDisposable TrapErrors<T>(this IObservable<T> source, ShellViewModel shell,
                                            Func<ErrorInfo, string>? translator = default, ILogger? logger = default)
        => source.GetErrorStream().TrapErrors(shell, translator ?? DefaultTranslator, logger);
}