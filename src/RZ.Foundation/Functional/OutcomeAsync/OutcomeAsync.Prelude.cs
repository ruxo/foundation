using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
// ReSharper disable InconsistentNaming

// ReSharper disable CheckNamespace
namespace RZ.Foundation;

public static partial class Prelude
{
    #region Catches

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static OutcomeAsyncCatch<T> matchError<T>(Func<Error, bool> predicate, Func<Error, OutcomeAsync<T>> fail) =>
        new(predicate, fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsyncCatch<T> ifFail<T>(OutcomeAsync<T> replacement) =>
        matchError(static _ => true, _ => replacement);

    public static OutcomeAsyncCatch<T> ifFail<T>(Func<Error, OutcomeAsync<T>> fail) =>
        matchError(static _ => true, fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsyncSideEffect failDo(Func<Error, Task<Unit>> fail) =>
        new(fail);

    #endregion

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsync<T> FailedOutcomeAsync<T>(Error error) =>
        LeftAsync<Error, T>(error);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsync<T> SuccessOutcomeAsync<T>(T value) =>
        RightAsync<Error, T>(value);

    public static readonly OutcomeAsync<Unit> unitOutcomeAsync = unit;
}