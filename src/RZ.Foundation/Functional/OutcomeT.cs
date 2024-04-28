using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LanguageExt;
using LanguageExt.Common;

namespace RZ.Foundation.Functional;

public sealed class SuccessT<IO, T>(HK<IO, T> value) : OutcomeT<IO, T> where IO : Functor<IO>, Monad<IO>, Eq<IO>
{
    public HK<IO, T> Value => value;
}

public sealed class FailureT<IO, T>(HK<IO, Error> error) : OutcomeT<IO, T> where IO : Functor<IO>, Monad<IO>, Eq<IO>
{
    public HK<IO, Error> Error => error;
}

public sealed class MaybeT<IO, T>(HK<IO, Outcome<T>> maybe) : OutcomeT<IO, T> where IO : Functor<IO>, Monad<IO>, Eq<IO>
{
    public HK<IO, Outcome<T>> Maybe => maybe;
}

public abstract class OutcomeT<IO, T> : HK<OutcomeX<IO>, T> where IO : Functor<IO>, Monad<IO>, Eq<IO>
{
    #region Pipe operators

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> operator |(OutcomeT<IO, T> ma, OutcomeT<IO, T> mb) =>
        ma.Catch(_ => mb).As();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> operator |(OutcomeT<IO, T> ma, CatchValue<T> @catch) =>
        ma.Catch(e => @catch.Match(e)
                          ? new SuccessT<IO, T>(IO.Return(@catch.Value(e)))
                          : new FailureT<IO, T>(IO.Return(e))).As();

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> operator |(OutcomeT<IO, T> ma, CatchError @error) =>
        ma.MapFailure(e => @error.Match(e) ? @error.Value(e) : e).As();

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

public class OutcomeX<IO> : Functor<OutcomeX<IO>>, Monad<OutcomeX<IO>>, ErrorHandlerable<OutcomeX<IO>>
    where IO : Functor<IO>,
    Monad<IO>,
    Eq<IO>
{
    #region ErrorHandlerable typeclass

    public static HK<OutcomeX<IO>, T> Catch<T>(HK<OutcomeX<IO>, T> ma, Func<Error, HK<OutcomeX<IO>, T>> handler) =>
        ma.As() switch
        {
            SuccessT<IO, T> s    => s,
            FailureT<IO, T> fail => new MaybeT<IO, T>(IO.Bind(fail.Error, e => handler(e).As().AsIo())),
            MaybeT<IO, T> m => new MaybeT<IO, T>(IO.Bind(m.Maybe, x => x.IfSuccess(out var v, out var e)
                                                                           ? IO.Return(SuccessOutcome(v))
                                                                           : handler(e).As().AsIo())),

            _ => throw new InvalidOperationException()
        };

    public static HK<OutcomeX<IO>, T> Catch<T>(HK<OutcomeX<IO>, T> ma, Func<Error, T> handler) =>
        ma.As() switch
        {
            SuccessT<IO, T> s    => s,
            FailureT<IO, T> fail => new SuccessT<IO, T>(IO.Map(fail.Error, handler)),
            MaybeT<IO, T> m      => new SuccessT<IO, T>(IO.Map(m.Maybe, x => x.Match(identity, handler))),

            _ => throw new InvalidOperationException()
        };

    public static HK<OutcomeX<IO>, T> MapFailure<T>(HK<OutcomeX<IO>, T> ma, Func<Error, Error> map) =>
        ma.As() switch
        {
            SuccessT<IO, T> s    => s,
            FailureT<IO, T> fail => new FailureT<IO, T>(IO.Map(fail.Error, map)),
            MaybeT<IO, T> m      => new MaybeT<IO, T>(IO.Map(m.Maybe, x => (Outcome<T>)x.Either.MapLeft(map))),

            _ => throw new InvalidOperationException()
        };

    #endregion

    public static HK<OutcomeX<IO>, B> Map<A, B>(HK<OutcomeX<IO>, A> ma, Func<A, B> f) =>
        ma.As() switch
        {
            SuccessT<IO, A> s    => new SuccessT<IO, B>(IO.Map(s.Value, f)),
            FailureT<IO, A> fail => new FailureT<IO, B>(fail.Error),
            MaybeT<IO, A> d      => new MaybeT<IO, B>(IO.Map(d.Maybe, x => x.Map(f).As())),

            _ => throw new InvalidOperationException()
        };

    #region Monad typeclass

    public static HK<OutcomeX<IO>, T> Return<T>(T value) =>
        new SuccessT<IO, T>(IO.Return(value));

    public static HK<OutcomeX<IO>, B> Bind<A, B>(HK<OutcomeX<IO>, A> ma, Func<A, HK<OutcomeX<IO>, B>> f) =>
        new MaybeT<IO, B>(ma.As() switch
                          {
                              // HK<IO,A> -> HK<IO, HK<OutcomeX<IO>, B>> -> HK<IO, OutcomeT<IO, B>> -> HK<IO, Outcome<B>>
                              SuccessT<IO, A> s    => IO.Bind(s.Value, a => f(a).As().AsIo()),
                              FailureT<IO, B> fail => IO.Map(fail.Error, FailedOutcome<B>),

                              // HK<IO, Outcome<A>> -map-> HK<IO, Outcome<HK<IO, Outcome<B>>> -> HK<IO, Outcome<B>>
                              MaybeT<IO, A> d =>
                                  IO.Bind(d.Maybe, x => x.Map(a => f(a).As().AsIo()).As().AsIo()),

                              _ => throw new InvalidOperationException()
                          });

    #endregion
}

public static class OutcomeT
{
    public static HK<IO, Outcome<T>> AsIo<IO, T>(this OutcomeT<IO, T> ma) where IO : Functor<IO>, Monad<IO>, Eq<IO> =>
        ma switch
        {
            SuccessT<IO, T> s    => IO.Map(s.Value, SuccessOutcome),
            FailureT<IO, T> fail => IO.Map(fail.Error, FailedOutcome<T>),
            MaybeT<IO, T> m      => m.Maybe,

            _ => throw new InvalidOperationException()
        };

    public static HK<IO, Outcome<T>> AsIo<IO, T>(this Outcome<HK<IO, Outcome<T>>> ma) where IO : Functor<IO>, Monad<IO>, Eq<IO> =>
        ma.Match(identity, e => IO.Return((Outcome<T>) e));
}

public class OutcomeFunctor : Functor<OutcomeFunctor>
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<OutcomeFunctor, B> Map<A, B>(HK<OutcomeFunctor, A> ma, Func<A, B> f) =>
        (Outcome<B>) ma.As().Either.Map(f);
}

public static class OutcomeExtensions
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> As<T>(this HK<OutcomeFunctor, T> @outcome) =>
        (Outcome<T>) @outcome;

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeT<IO, T> As<IO, T>(this HK<OutcomeX<IO>, T> @outcome)
        where IO : Functor<IO>, Monad<IO>, Eq<IO> =>
        (OutcomeT<IO, T>) @outcome;
}
