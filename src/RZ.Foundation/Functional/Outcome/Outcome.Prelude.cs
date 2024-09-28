using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using RZ.Foundation.Types;

// ReSharper disable InconsistentNaming

// ReSharper disable CheckNamespace
namespace RZ.Foundation;

public static partial class Prelude
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static OutcomeCatch<T> matchError<T>(Func<ErrorInfo, bool> predicate, Func<ErrorInfo, Outcome<T>> fail) =>
        new(predicate, fail);

    #region catch

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(Outcome<T> replacement)
        => new(static _ => true, _ => replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(Func<ErrorInfo, T> replacement)
        => new(static _ => true, e => replacement(e));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(Func<ErrorInfo, ErrorInfo> replacement)
        => new(static _ => true, e => replacement(e));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(Func<ErrorInfo, Outcome<T>> replacement)
        => new(static _ => true, replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(ErrorInfo error, Outcome<T> replacement)
        => matchError(e => e.Is(error), e => replacement);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(ErrorInfo error, Func<ErrorInfo, T> @catch)
        => matchError(e => e.Is(error), e => SuccessOutcome(@catch(e)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeCatch<T> @catch<T>(ErrorInfo error, Func<ErrorInfo, ErrorInfo> replacement)
        => matchError(e => e.Is(error), e => (Outcome<T>) replacement(e));

    #endregion

    #region failDo

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect failDo(Action<ErrorInfo> fail) =>
        new(ToUnit(fail));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect failDo(Func<ErrorInfo, Unit> fail) =>
        new(fail);

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect<T> @do<T>(Action<T> sideEffect) =>
        new(ToUnit(sideEffect));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeSideEffect<T> @do<T>(Func<T, Unit> sideEffect) =>
        new(sideEffect);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> FailedOutcome<T>(ErrorInfo error) => error;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> SuccessOutcome<T>(T value) => value;

    public static readonly Outcome<Unit> UnitOutcome = unit;
}