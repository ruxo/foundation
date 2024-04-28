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

    public static readonly OutcomeT<Synchronous, Unit> UnitOutcome = Success(unit);
    public static readonly OutcomeT<Asynchronous, Unit> UnitOutcomeAsync = SuccessAsync(unit);

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
    public static OutcomeT<Asynchronous, T> SuccessAsync<T>(T value) =>
        new SuccessT<Asynchronous, T>(Asynchronous.Return(value));

    [Pure]
    public static OutcomeT<Asynchronous, T> FailureAsync<T>(Error error) =>
        new FailureT<Asynchronous, T>(Asynchronous.Return(error));

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

    public static OutcomeT<Asynchronous, T> TryCatch<T>(Func<Task<Outcome<T>>> handler) where T: notnull {
        var @wrap = new FunctionAsyncYield<Outcome<T>>(async () => await handler());
        var @try = Asynchronous.Try(() => @wrap);
        return new MaybeT<Asynchronous, T>(Asynchronous.Map(@try, x => x.Bind(identity)));
    }

    public static OutcomeT<Asynchronous, T> TryCatch<T>(Func<Task<T>> handler) where T: notnull {
        var @wrap = new FunctionAsyncYield<T>(async () => await handler());
        var @try = Asynchronous.Try(() => @wrap);
        return new MaybeT<Asynchronous, T>(@try);
    }

    public static OutcomeT<Asynchronous, Unit> TryCatch(Func<Task> handler) {
        var @wrap = new FunctionAsyncYield<Unit>(async () => {
                                                     await handler();
                                                     return unit;
                                                 });
        var @try = Asynchronous.Try(() => @wrap);
        return new MaybeT<Asynchronous, Unit>(@try);
    }

    public static OutcomeT<IO, T> TryCatch<IO, T>(Func<OutcomeT<IO, T>> handler) where IO : IOT<IO> {
        var @try = IO.Try(() => handler().AsIo());
        var iop = @try.Map(x => x.Bind(identity));
        return new MaybeT<IO, T>(iop);
    }

    public static OutcomeT<Synchronous, T> TryCatch<T>(Func<T> handler) where T: notnull {
        var @try = Synchronous.Try(() => Synchronous.Return(handler()));
        return new MaybeT<Synchronous, T>(@try);
    }


    public static OutcomeT<Synchronous, Unit> TryCatch(Action handler) =>
        new MaybeT<Synchronous, Unit>(Synchronous.Try(() => {
                                                          handler();
                                                          return Synchronous.Return(unit);
                                                      }));

    #endregion
}