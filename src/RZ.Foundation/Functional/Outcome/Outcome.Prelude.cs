using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LanguageExt;
using LanguageExt.Common;
// ReSharper disable InconsistentNaming

// ReSharper disable CheckNamespace
namespace RZ.Foundation;

public static partial class Prelude
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static OutcomeCatch<T> matchError<T>(Func<Error, bool> predicate, Func<Error, Outcome<T>> fail) =>
        new(predicate, fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> ifFail<T>(Outcome<T> replacement) =>
        new(static _ => true, _ => replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> ifFail<T>(Func<Error, Outcome<T>> fail) =>
        matchError(static _ => true, fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CatchValue<T> ifFail<T>(Func<Error, T> fail) =>
        new(static _ => true, fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CatchValue<T> ifFail<T>(Error error, T replacement) =>
        new(e => e.Is(error), _ => replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CatchError ifFail(Error error, Error replacement) =>
        new(e => e.Is(error), _ => replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CatchError ifFail(Func<Error, Error> fail) =>
        new(static _ => true, fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CatchError ifFail(Error error, Func<Error, Error> handler) =>
        new(e => e.Is(error), handler);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> ifFail<T>(Error error, Func<Error, T> @catch) =>
        matchError(e => e.Is(error), e => SuccessOutcome(@catch(e)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect failDo(Action<Error> fail) =>
        new(ToUnit(fail));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect failDo(Func<Error, Unit> fail) =>
        new(fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect<T> @do<T>(Action<T> sideEffect) =>
        new(ToUnit(sideEffect));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect<T> @do<T>(Func<T,Unit> sideEffect) =>
        new(sideEffect);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> FailedOutcome<T>(Error error) => error;

    /// <summary>
    /// Transform any action into Outcome<Unit>, the action SHOULD NOT throw exceptions.
    /// Only work with Outcome<Unit>
    /// </summary>
    public static OutcomeCatch<Unit> @ifFail(Action<Error> fail) =>
        matchError(static _ => true, e => {
                                         fail(e);
                                         return (Outcome<Unit>) unit;
                                     });

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> SuccessOutcome<T>(T value) => value;

    public static readonly Outcome<Unit> unitOutcome = unit;
}