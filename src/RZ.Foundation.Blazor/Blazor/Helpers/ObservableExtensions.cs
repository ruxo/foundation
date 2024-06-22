using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
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
        stream.Select(_ => unit);

    /// <summary>
    /// Get the execution handler for a reactive command
    /// </summary>
    public static Func<Task<T>> OnExecute<T>(this ReactiveCommand<Unit, T> command) => async () =>
        await command.Execute();

    public static Func<Task<TOut>> OnExecute<TIn,TOut>(this ReactiveCommand<TIn, TOut> command, Func<TIn> value) => async () =>
        await command.Execute(value());

    public static IObservable<T> Shared<T>(this IObservable<T> coldObservable) =>
        coldObservable.Publish().RefCount();
}