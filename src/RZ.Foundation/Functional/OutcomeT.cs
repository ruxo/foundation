using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RZ.Foundation.Types;

namespace RZ.Foundation.Functional;

public sealed class SuccessT<IO, T>(HK<IO, T> value) : OutcomeT<IO, T>
    where IO : IOT<IO>
{
    public HK<IO, T> Value => value;
}

public sealed class FailureT<IO, T>(HK<IO, ErrorInfo> error) : OutcomeT<IO, T>
    where IO : IOT<IO>
{
    public HK<IO, ErrorInfo> ErrorInfo => error;
}

public sealed class MaybeT<IO, T>(HK<IO, Outcome<T>> maybe) : OutcomeT<IO, T>
    where IO : IOT<IO>

{
    public HK<IO, Outcome<T>> Maybe => maybe;
}

public abstract class OutcomeT<IO, T> : HK<OutcomeX<IO>, T>
    where IO : IOT<IO>
{
    #region Pipe operators

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> operator |(OutcomeT<IO, T> ma, OutcomeT<IO, T> mb) =>
        ma.Catch(_ => mb).As();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> operator |(OutcomeT<IO, T> ma, OutcomeSideEffect sideEffect) =>
        ma.MapFailure(e => {
            sideEffect.Run(e);
            return e;
        }).As();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> operator |(OutcomeT<IO, T> ma, OutcomeSideEffect<T> sideEffect) =>
        ma.Map(e => {
            sideEffect.Run(e);
            return e;
        }).As();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, Unit> operator |(OutcomeT<IO, T> ma, OutcomeCatch<Unit> sideEffect) =>
        new MaybeT<IO, Unit>(ma.AsIo().Map(x => x.Match(_ => unit, sideEffect.Run)));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> operator |(OutcomeT<IO, T> ma, OutcomeCatch<T> sideEffect) =>
        ma.Catch(e => {
            var v = sideEffect.Run(e);
            return new MaybeT<IO, T>(IO.Return(v));
        }).As();

    #endregion

    #region Equality typeclass

    public HK<IO, bool> EqualsTo(OutcomeT<IO, T> another) =>
        IO.EqualsTo(this.AsIo(), another.AsIo());

    public HK<IO, bool> NotEqualsTo(OutcomeT<IO, T> another) =>
        IO.NotEqualsTo(this.AsIo(), another.AsIo());

    #endregion
}

public static class OutcomeT
{
    [Pure]
    public static HK<IO, Outcome<T>> AsIo<IO, T>(this OutcomeT<IO, T> ma)
        where IO : IOT<IO>
        => ma switch {
            SuccessT<IO, T> s    => IO.Map(s.Value, SuccessOutcome),
            FailureT<IO, T> fail => IO.Map(fail.ErrorInfo, FailedOutcome<T>),
            MaybeT<IO, T> m      => m.Maybe,

            _ => throw new InvalidOperationException()
        };

    [Pure]
    public static HK<IO, Outcome<T>> AsIo<IO, T>(this Outcome<HK<IO, Outcome<T>>> ma)
        where IO : IOT<IO>
        => ma.Match(identity, e => IO.Return((Outcome<T>)e));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<OutcomeX<Synchronous>, T> AsIo<T>(this Outcome<T> ma)
        => (OutcomeT<Synchronous, T>)ma;
}

public class OutcomeFunctor : Functor<OutcomeFunctor>
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<OutcomeFunctor, B> Map<A, B>(HK<OutcomeFunctor, A> ma, Func<A, B> f)
        => ma.As().Map(f);
}

public static class OutcomeExtensions
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> As<T>(this HK<OutcomeFunctor, T> @outcome) =>
        (Outcome<T>)@outcome;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> As<IO, T>(this HK<OutcomeX<IO>, T> @outcome)
        where IO : IOT<IO>
        => (OutcomeT<IO, T>)@outcome;
}

public readonly struct OutcomeAsyncCatch<T>(Func<ErrorInfo, OutcomeT<Asynchronous, T>> fail)
{
    public OutcomeAsyncCatch(Func<ErrorInfo, bool> predicate, Func<ErrorInfo, OutcomeT<Asynchronous, T>> fail)
        : this(e => predicate(e) ? fail(e) : FailureAsync<T>(e)) {
    }

    public OutcomeT<Asynchronous, T> Run(ErrorInfo error) => fail(error);
}

public readonly struct OutcomeAsyncSideEffect(Func<ErrorInfo, Task<Unit>> sideEffect)
{
    public Task<Unit> Run(ErrorInfo error) => sideEffect(error);
}