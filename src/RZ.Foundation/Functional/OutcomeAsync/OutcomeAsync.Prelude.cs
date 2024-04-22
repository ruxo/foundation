using System;
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

    /// <summary>
    /// Transform any action into OutcomeAsync<Unit>, the action SHOULD NOT throw exceptions.
    /// </summary>
    public static OutcomeAsyncCatch<Unit> @uncheckedOutcomeAsync(Action<Error> fail) =>
        matchError(static _ => true, e => {
                                         fail(e);
                                         return (OutcomeAsync<Unit>) unit;
                                     });

    #endregion

    public static OutcomeAsync<T> OutcomeAsync<T>(Task<T> f) =>
        EitherAsync<Error, T>.RightAsync(f);

    public static readonly OutcomeAsync<Unit> unitOutcomeAsync = unit;
}