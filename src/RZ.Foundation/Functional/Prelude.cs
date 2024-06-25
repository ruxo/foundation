using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RZ.Foundation.Functional;
using RZ.Foundation.Types;

// ReSharper disable InconsistentNaming

// ReSharper disable CheckNamespace

namespace RZ.Foundation;

public static partial class Prelude
{
    #region OutcomeT

    public static readonly OutcomeT<Synchronous, Unit> UnitOutcome = Success(unit);
    public static readonly OutcomeT<Asynchronous, Unit> UnitOutcomeAsync = SuccessAsync(unit);

    #region Success / Failure

    [Pure]
    public static OutcomeT<Synchronous, T> Success<T>(Func<T> value) =>
        new SuccessT<Synchronous, T>(new FunctionYield<T>(value));

    [Pure]
    public static OutcomeT<Synchronous, T> Success<T>(T value) =>
        new SuccessT<Synchronous, T>(Synchronous.Return(value));

    [Pure]
    public static OutcomeT<Synchronous, T> Failure<T>(ErrorInfo error) =>
        new FailureT<Synchronous, T>(Synchronous.Return(error));

    [Pure]
    public static OutcomeT<Asynchronous, T> SuccessAsync<T>(T value) =>
        new SuccessT<Asynchronous, T>(Asynchronous.Return(value));

    [Pure]
    public static OutcomeT<Asynchronous, T> SuccessAsync<T>(Func<Task<T>> value) =>
        new SuccessT<Asynchronous, T>(new FunctionAsyncYield<T>(async () => await value()));

    [Pure]
    public static OutcomeT<Asynchronous, T> FailureAsync<T>(ErrorInfo error) =>
        new FailureT<Asynchronous, T>(Asynchronous.Return(error));

    #endregion

    #region ToOutcome

    [Pure]
    public static OutcomeT<Synchronous, T> ToOutcome<T>(this Option<T> opt, ErrorInfo? error = default) =>
        opt.Match(Success, () => Failure<T>(error ?? new(StandardErrorCodes.NotFound)));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<Synchronous, T> ToOutcome<T>(this Either<ErrorInfo, T> opt) =>
        opt.Match(Success, Failure<T>);

    [Pure]
    public static OutcomeT<Synchronous, T> ToOutcome<T>(this Try<T> self) =>
        self.ToEither(e => ErrorFrom.Exception(e)).ToOutcome();

    #endregion

    #region Catches

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static OutcomeAsyncCatch<T> matchError<T>(Func<ErrorInfo, bool> predicate, Func<ErrorInfo, OutcomeT<Asynchronous, T>> fail)
        =>
            new(predicate, fail);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsyncCatch<T> ifFail<T>(OutcomeT<Asynchronous, T> replacement) =>
        matchError(static _ => true, _ => replacement);

    public static OutcomeAsyncCatch<T> ifFail<T>(Func<ErrorInfo, OutcomeT<Asynchronous, T>> fail) =>
        matchError(static _ => true, fail);

    #endregion

    #endregion

    #region IOT

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<Asynchronous, T> AsyncValue<T>(T value) =>
        Asynchronous.Return(value);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<Asynchronous, T> AsyncValue<T>(Task<T> value) =>
        new FunctionAsyncYield<T>(async () => await value);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<Asynchronous, T> AsyncValue<T>(Func<Task<T>> f) =>
        new FunctionAsyncYield<T>(async () => await f());

    #endregion

    #region Try/Catch

    public static OutcomeT<Asynchronous, T> TryCatch<T>(Func<Task<Outcome<T>>> handler) where T : notnull {
        var @wrap = new FunctionAsyncYield<Outcome<T>>(async () => await handler());
        var @try = Asynchronous.Try(() => @wrap);
        return new MaybeT<Asynchronous, T>(Asynchronous.Map(@try, x => x.Bind(identity)));
    }

    public static OutcomeT<Asynchronous, T> TryCatch<T>(Func<Task<T>> handler) where T : notnull {
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

    public static OutcomeT<IO, T> TryCatch<IO, T>(Func<OutcomeT<IO, T>> handler)
        where IO : IOT<IO> {
        var @try = IO.Try(() => handler().AsIo());
        var iop = @try.Map(x => x.Bind(identity));
        return new MaybeT<IO, T>(iop);
    }

    public static OutcomeT<Synchronous, T> TryCatch<T>(Func<T> handler) where T : notnull {
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