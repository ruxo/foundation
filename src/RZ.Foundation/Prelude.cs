global using static RZ.Foundation.Prelude;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LanguageExt.Common;
using RZ.Foundation.Types;

namespace RZ.Foundation;

[PublicAPI]
public static partial class Prelude {

    #region LanguageExt forward

    public static readonly Unit unit = Unit.Default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Seq<T> Seq<T>(IEnumerable<T> items) => LanguageExt.Prelude.Seq(items);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Seq<T> Seq<T>(params T[] p) => LanguageExt.Prelude.Seq(p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T v) => LanguageExt.Prelude.Some(v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Some<T>(T? v) where T: struct => LanguageExt.Prelude.Some(v);

    public static readonly OptionNone None = LanguageExt.Prelude.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Optional<T>(T? value) => LanguageExt.Prelude.Optional(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Optional<T>(T? value) where T: struct => LanguageExt.Prelude.Optional(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T identity<T>(T x) => x;

    #endregion


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
        return new ErrorInfo(code, ex.Message, ex.ToString(), data: null, ex.InnerException?.ToErrorInfo(), stack: ex.StackTrace);
    }

    public static ErrorInfo ToErrorInfo(this Error e) {
        if (e.Exception.ToNullable() is ErrorInfoException ei) return ei.ToErrorInfo();

        var errorInfoAttr = from ex in e.Exception
                            from attr in Optional(ex.GetType().GetCustomAttribute<ErrorInfoAttribute>())
                            select attr.Code;
        var subErrors = e is ManyErrors me ? me.Errors.Map(ToErrorInfo) : LanguageExt.Seq.empty<ErrorInfo>();
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

    #region On & Try

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

    public static (Exception? Error, Unit Value) Try<S>(S state, Action<S> f) {
        try{
            f(state);
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static (Exception? Error, Unit Value) Try(Action f)
    {
        try{
            f();
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static async Task<(Exception? Error, Unit Value)> Try(Func<Task> f)
    {
        try
        {
            await f();
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static async Task<(Exception? Error, Unit Value)> Try<S>(S state, Func<S, Task> f)
    {
        try
        {
            await f(state);
            return (null, unit);
        }
        catch (Exception e){
            return (e, unit);
        }
    }

    public static (Exception? Error, T Value) Try<S,T>(S state, Func<S,T> f)
    {
        try
        {
            return (null, f(state));
        }
        catch (Exception e)
        {
            return (e, default!);
        }
    }

    public static (Exception? Error, T Value) Try<T>(Func<T> f)
    {
        try
        {
            return (null, f());
        }
        catch (Exception e)
        {
            return (e, default!);
        }
    }

    public static async Task<(Exception? Error, T Value)> Try<T>(Func<Task<T>> f)
    {
        try
        {
            return (null, await f());
        }
        catch (Exception e)
        {
            return (e, default!);
        }
    }

    public static async Task<(Exception? Error, T Value)> Try<S,T>(S state, Func<S, Task<T>> f)
    {
        try
        {
            return (null, await f(state));
        }
        catch (Exception e)
        {
            return (e, default!);
        }
    }

    [Pure]
    public static Option<T> ToOption<T>(this in (Exception?, T) result)
        => result switch {
            (null, var value) => value,
            (_, _)            => None
        };

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<T>> ToOption<T>(this Task<(Exception?, T)> result)
        => (await result).ToOption();

    [Pure]
    public static Outcome<T> ToOutcome<T>(this in (Exception?, T) result)
        => result switch {
            (null, var value) => value,
            var (error, _)    => ErrorFrom.Exception(error)
        };

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Outcome<T>> ToOutcome<T>(this Task<(Exception?, T)> result)
        => (await result).ToOutcome();

    [Pure]
    public static Result<T> ToResult<T>(this in (Exception?, T) result)
        => result switch {
            (null, var value) => value,
            var (error, _)    => new Result<T>(error)
        };

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Result<T>> ToResult<T>(this Task<(Exception?, T)> result)
        => (await result).ToResult();

    #endregion
}