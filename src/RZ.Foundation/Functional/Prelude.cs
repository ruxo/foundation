using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using RZ.Foundation.Functional;

// ReSharper disable CheckNamespace

namespace RZ.Foundation;

public static partial class Prelude
{
    #region Outcome

    [Pure]
    public static OutcomeT<Synchronous, T> Success<T>(Func<T> value) =>
        new SuccessT<Synchronous, T>(new FunctionYield<T>(value));

    [Pure]
    public static OutcomeT<Synchronous, T> Success<T>(T value) =>
        new SuccessT<Synchronous, T>(Synchronous.Return(value));

    [Pure]
    public static OutcomeT<Synchronous, T> Failure<T>(Error error) =>
        new FailureT<Synchronous, T>(Synchronous.Return(error));

    [Pure]
    public static OutcomeT<Synchronous, T> ToOutcome<T>(this Option<T> opt, Error? error = default) =>
        opt.Match(Success, () => Failure<T>(error ?? StandardErrors.NotFound));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<Synchronous, T> ToOutcome<T>(this Either<Error, T> opt) =>
        opt.Match(Success, Failure<T>);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<Synchronous, T> ToOutcome<T>(this Try<T> self) =>
        self.ToEither(Error.New).ToOutcome();

    #endregion

    #region Try/Catch

    public static OutcomeAsync<T> TryCatch<T>(Func<Task<Outcome<T>>> handler) =>
        from v in TryAsync(handler).ToOutcome()
        from result in v.ToAsync()
        select result;

    public static OutcomeAsync<T> TryCatch<T>(Func<Task<Either<Error, T>>> handler) =>
        from v in TryAsync(handler).ToEither()
        from result in v.ToAsync()
        select result;

    public static OutcomeAsync<T> TryCatch<T>(Func<Task<T>> handler) =>
        TryAsync(handler).ToOutcome();

    public static OutcomeAsync<Unit> TryCatch(Func<Task> handler) =>
        TryAsync(async () => {
                     await handler();
                     return Unit.Default;
                 }).ToEither();

    public static Outcome<T> TryCatch<T>(Func<Outcome<T>> handler) =>
        Try(handler).ToEither().Match(identity, e => FailedOutcome<T>(e));

    public static Outcome<T> TryCatch<T>(Func<Either<Error, T>> handler) =>
        Try(handler).ToEither(Error.New).Bind(identity);

    public static Outcome<T> TryCatch<T>(Func<T> handler) =>
        Try(handler).ToEither(Error.New);

    public static Outcome<Unit> TryCatch(Action handler) =>
        Try(() => {
                handler();
                return unit;
            }).ToEither(Error.New);

    #endregion
}