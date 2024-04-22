// ReSharper disable CheckNamespace

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LanguageExt;
using LanguageExt.Common;
using RZ.Foundation.Extensions;

// ReSharper disable InconsistentNaming

namespace RZ.Foundation;

public readonly struct Outcome<T>
{
    internal readonly Either<Error, T> value;

    Outcome(Either<Error, T> value) => this.value = value;

    public static implicit operator Outcome<T>(T value) => new(value);
    public static implicit operator Outcome<T>(Error value) => new(value);
    public static implicit operator Outcome<T>(Either<Error, T> value) => new(value);

    public bool IsFail => value.IsLeft;
    public bool IsSuccess => value.IsRight;

    internal Either<Error, T> Either => value;

    #region Pipe operators

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> operator |(Outcome<T> ma, in Outcome<T> mb) =>
        ma.value | mb.value;

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeCatch<T> mb) =>
        ma.value.BindLeft(e => mb.Run(e).Either);

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeSideEffect<T> sideEffect) =>
        ma.value.Map(e => {
                         sideEffect.Run(e);
                         return e;
                     });

    public static Outcome<T> operator |(Outcome<T> ma, OutcomeSideEffect sideEffect) =>
        ma.value.MapLeft(e => {
                             sideEffect.Run(e);
                             return e;
                         });

    public static Outcome<T> operator | (Outcome<T> ma, CatchValue<T> mb) =>
        ma.value.BindLeft(e => mb.Match(e)? SuccessOutcome(mb.Value(e)).Either : e);

    public static Outcome<T> operator | (Outcome<T> ma, CatchError mb) =>
        ma.value.BindLeft(e => mb.Match(e)? FailedOutcome<T>(mb.Value(e)).Either : e);

    #endregion

    [Pure]
    public OutcomeAsync<T> ToAsync() => value.ToAsync();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Outcome<T> Catch(Func<Error, T> handler) =>
        value.Match(identity, handler);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Outcome<T> Catch(Func<Error, Error> handler) =>
        value.BiMap(identity, handler);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T IfFail(Func<Error, T> mapper) => value.IfLeft(mapper);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T IfFail(T defaultValue) => value.IfLeft(defaultValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Unit IfFail(Action<Error> fail) => value.IfLeft(fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IfFail(out Error e, out T v) => value.IfLeft(out e, out v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IfSuccess(out T v, out Error e) => value.IfRight(out v, out e);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Unwrap() =>
        value.Match(identity, JustThrow);

    [ExcludeFromCodeCoverage, MethodImpl(MethodImplOptions.AggressiveInlining)]
    static T JustThrow(Error e) {
        e.Throw();
        return default!;
    }

    public Error UnwrapError() =>
        value.Match(_ => throw new InvalidOperationException("Outcome state is success"), identity);
}

public readonly struct OutcomeCatch<T>(Func<Error, Outcome<T>> fail)
{
    public OutcomeCatch(Func<Error, bool> predicate, Func<Error, Outcome<T>> fail)
        : this(e => predicate(e) ? fail(e) : e){}

    public Outcome<T> Run(Error error) => fail(error);
}

public readonly struct OutcomeSideEffect<T>(Func<T, Unit> sideEffect)
{
    public Unit Run(T data) => sideEffect(data);
}

public readonly struct OutcomeSideEffect(Func<Error, Unit> sideEffect)
{
    public Unit Run(Error error) => sideEffect(error);
}
