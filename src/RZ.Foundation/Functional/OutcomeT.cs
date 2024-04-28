using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace RZ.Foundation.Functional;

public static class OutcomeIO
{
    public static Outcome<T> RunIO<T>(this OutcomeT<Synchronous, T> ma) =>
        ma.AsIo().RunIO();

    public static ValueTask<Outcome<T>> RunIO<T>(this OutcomeT<Asynchronous, T> ma) =>
        ma.AsIo().RunIO();
}

public record SuccessT<IO, T>(HK<IO, T> Value) : OutcomeT<IO, T> where IO : Functor<IO>, Monad<IO>, Eq<IO>;

public record FailureT<IO, T>(HK<IO, Error> Error) : OutcomeT<IO, T> where IO : Functor<IO>, Monad<IO>, Eq<IO>;

public record MaybeT<IO, T>(HK<IO, Outcome<T>> Maybe) : OutcomeT<IO, T> where IO : Functor<IO>, Monad<IO>, Eq<IO>;

public abstract record OutcomeT<IO, T> : HK<OutcomeX<IO>, T> where IO : Functor<IO>, Monad<IO>, Eq<IO>
{
    public HK<IO, bool> EqualsTo(OutcomeT<IO, T> another) =>
        IO.EqualsTo(this.AsIo(), another.AsIo());

    public HK<IO, bool> NotEqualsTo(OutcomeT<IO, T> another) =>
        IO.NotEqualsTo(this.AsIo(), another.AsIo());
}

public class OutcomeX<IO> : Functor<OutcomeX<IO>>, Monad<OutcomeX<IO>>, ErrorHandlerable<OutcomeX<IO>>
    where IO : Functor<IO>,
    Monad<IO>,
    Eq<IO>
{
    public static HK<OutcomeX<IO>, T> Catch<T>(HK<OutcomeX<IO>, T> ma, Func<Error, Error> handler) {
        return ma switch {
            SuccessT<IO, T> s    => s,
            FailureT<IO, T> fail => new FailureT<IO, T>(IO.Map(fail.Error, handler)),
            MaybeT<IO, T> m      => new MaybeT<IO, T>(IO.Map(m.Maybe, x => (Outcome<T>) x.Either.MapLeft(handler))),

            _ => throw new InvalidOperationException()
        };
    }

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
    public static OutcomeT<Asynchronous, C> SelectMany<A, B, C>(this OutcomeT<Synchronous, A> ma, Func<A, OutcomeT<Asynchronous, B>> bind,
                                                          Func<A, B, C> project) {
        return new MaybeT<Asynchronous, C>(new ConstantAsyncYield<Outcome<C>>(SyncToAsync()));
        async ValueTask<Outcome<C>> SyncToAsync() {
            if (ma.AsIo().RunIO().IfSuccess(out var a, out var e)) {
                var ba = await bind(a).AsIo().RunIO();
                return ba.Map(b => project(a, b)).As();
            }
            else
                return e;
        }
    }

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
