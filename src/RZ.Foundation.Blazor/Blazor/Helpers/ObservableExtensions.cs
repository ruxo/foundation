using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RZ.Foundation.Blazor.Layout;
using RZ.Foundation.Blazor.Layout.Shell;
using RZ.Foundation.Types;
using Unit = LanguageExt.Unit;

namespace RZ.Foundation.Blazor.Helpers;

public static class ObservableExtensions
{
    static ErrorInfo From<T>(Notification<T> r)
        => r.Exception is ErrorInfoException ei
               ? new ErrorInfo(ei.Code, ei.Message)
               : new ErrorInfo(StandardErrorCodes.Unhandled, r.Exception!.ToString());


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

    static string DefaultTranslator(ErrorInfo e) => e.Message;

    static IDisposable TrapErrors(this IObservable<ErrorInfo> source, ShellViewModel shell,
                                  Func<ErrorInfo, string> translator, ILogger? logger = default) =>
        source.Subscribe(e => {
            if (logger is null)
                Trace.WriteLine($"TrapErrors: {e.Code}:{e.Message}");
            else
                logger.LogError("TrapErrors: {@Error}", e);

            shell.Notify(new(Severity.Error, translator(e)));
        });

    public static IDisposable TrapErrors<T>(this IObservable<Outcome<T>> source, ShellViewModel shell,
                                            Func<ErrorInfo, string>? translator = default, ILogger? logger = default) =>
        source.GetErrorStream().TrapErrors(shell, translator ?? DefaultTranslator, logger);

    public static IDisposable TrapErrors<T>(this IObservable<T> source, ShellViewModel shell,
                                            Func<ErrorInfo, string>? translator = default, ILogger? logger = default) =>
        source.GetErrorStream().TrapErrors(shell, translator ?? DefaultTranslator, logger);
}