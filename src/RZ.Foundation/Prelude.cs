global using static LanguageExt.Prelude;
global using static RZ.Foundation.Prelude;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LanguageExt.Common;
using RZ.Foundation.Types;
using Seq = LanguageExt.Seq;

namespace RZ.Foundation;

[PublicAPI]
public static partial class Prelude {
    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T> constant<T>(T x) => () => x;

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.NoOptimization)]
    public static void Noop() { }

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, T> SideEffect<T>(Action<T> f) => x => {
                                                               f(x);
                                                               return x;
                                                           };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, Task<T>> SideEffectAsync<T>(Func<T, Task> f) => async x => {
                                                                              await f(x);
                                                                              return x;
                                                                          };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SideEffect<T>(this T x, Action<T> f) {
        f(x);
        return x;
    }

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> SideEffectAsync<T>(this T x, Func<T,Task> f) {
        await f(x);
        return x;
    }

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<Unit> BooleanToOption(bool b) => b ? unit : None;

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<Unit> ToUnit<T>(Func<T> effect) =>
        () => {
            effect();
            return unit;
        };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Func<T, Unit> ToUnit<T>(Action<T> action) =>
        v => {
            action(v);
            return unit;
        };

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Unit ToUnit(Action action) {
        action();
        return unit;
    }

    public static Option<(A, B)> With<A, B>(Option<A> a, Option<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));
    public static Option<(A, B, C)> With<A, B, C>(Option<A> a, Option<B> b, Option<C> c) =>
        a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx,cx))));

    public static Result<(A, B)> With<A, B>(Result<A> a, Result<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));
    public static Result<(A, B, C)> With<A, B, C>(Result<A> a, Result<B> b, Result<C> c)
        => a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx,cx))));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> ThrowIfError<T>(Task<Outcome<T>> value)
        => (await value).Unwrap();

    public static T ThrowIfNotFound<T>(this Option<T> optionValue, string message)
        => optionValue.GetOrThrow(() => new ErrorInfoException("not-found", message));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ThrowIfNotFound<T>(Option<T> value)
        => value.ThrowIfNotFound("Not found");

    [PublicAPI]
    public static ErrorInfo NoDebug(this ErrorInfo ei)
        => ei.With(debug: None, stack: default(ErrorInfo.StackInfo), inner: None, subErrors: None);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ErrorInfo With(this ErrorInfo ei, string? code = null, Option<string>? debug = null, ErrorInfo.StackInfo? stack = null,
                                 Option<ErrorInfo>? inner = null, Option<Seq<ErrorInfo>>? subErrors = null)
        => new(code ?? ei.Code, ei.Message, (debug ?? Optional(ei.DebugInfo)).ToNullable(), ei.Data,
               (inner ?? ei.InnerError).ToNullable(),
               subErrors is null ? ei.SubErrors : subErrors.Value.ToNullable(),
               (stack ?? ei.Stack).Value, ei.TraceId);

    public static ErrorInfo ToErrorInfo(this Exception ex) {
        if (ex is ErrorInfoException ei) return ei.ToErrorInfo();

        var errorInfoAttr = ex.GetType().GetCustomAttribute<ErrorInfoAttribute>()?.Code;
        var code = errorInfoAttr ?? ex.GetType().FullName ?? StandardErrorCodes.Unhandled;
        return new ErrorInfo(code, ex.Message, ex.ToString(), data: default, ex.InnerException?.ToErrorInfo(), stack: ex.StackTrace);
    }

    public static ErrorInfo ToErrorInfo(this Error e) {
        if (e.Exception.ToNullable() is ErrorInfoException ei) return ei.ToErrorInfo();

        var errorInfoAttr = from ex in e.Exception
                            from attr in Optional(ex.GetType().GetCustomAttribute<ErrorInfoAttribute>())
                            select attr.Code;
        var subErrors = e is ManyErrors me ? me.Errors.Map(ToErrorInfo) : Seq.empty<ErrorInfo>();
        var code = errorInfoAttr.OrElse(() => e.Exception.Map(ex => ex.GetType().FullName)).IfNone(StandardErrorCodes.Unhandled)!;
        return new ErrorInfo(code,
                             e.Message,
                             e.Exception.ToNullable()?.ToString(),
                             data: default,
                             e.Inner.Map(ToErrorInfo).ToNullable(),
                             subErrors.IsEmpty ? null : subErrors,
                             stack: e.Exception.ToNullable()?.StackTrace);
    }

    public static B? Apply<A,B>(this A? a, Func<A,B> f)
        where A: class where B: class
        => a is null ? null : f(a);

    public static B? Bind<A,B>(this A? a, Func<A,B?> f)
        where A: class where B: class
        => a is null ? null : f(a);

    public static B? Apply<A,B>(this A? a, Func<A,B> f)
        where A: struct where B: class
        => a is null ? null : f(a.Value);

    public static B? Bind<A,B>(this A? a, Func<A,B?> f)
        where A: struct where B: class
        => a is null ? null : f(a.Value);

    public static B? ApplyValue<A,B>(this A? a, Func<A,B> f)
        where A: class where B: struct
        => a is null ? null : f(a);

    public static B? BindValue<A,B>(this A? a, Func<A,B?> f)
        where A: class where B: struct
        => a is null ? null : f(a);

    public static B? ApplyValue<A,B>(this A? a, Func<A,B> f)
        where A: struct where B: struct
        => a is null ? null : f(a.Value);

    public static B? BindValue<A,B>(this A? a, Func<A,B?> f)
        where A: struct where B: struct
        => a is null ? null : f(a.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandlerSync On(Action task)
        => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandlerSync<T> On<T>(Func<T> task)
        => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandler On(Task task)
        => new(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OnHandler<T> On<T>(Task<T> task)
        => new(task);
}