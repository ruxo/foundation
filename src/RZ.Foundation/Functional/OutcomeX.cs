using System;
using System.Diagnostics.Contracts;
using RZ.Foundation.Types;
// ReSharper disable InconsistentNaming

namespace RZ.Foundation.Functional;

public class OutcomeX<IO> : Functor<OutcomeX<IO>>, Monad<OutcomeX<IO>>, ErrorHandlerable<OutcomeX<IO>>
    where IO : IOT<IO>
{
    #region ErrorHandlerable typeclass

    public static HK<OutcomeX<IO>, T> Catch<T>(HK<OutcomeX<IO>, T> ma, Func<ErrorInfo, HK<OutcomeX<IO>, T>> handler) =>
        ma.As() switch {
            SuccessT<IO, T> s    => s,
            FailureT<IO, T> fail => new MaybeT<IO, T>(IO.Bind(fail.ErrorInfo, e => handler(e).As().AsIo())),
            MaybeT<IO, T> m => new MaybeT<IO, T>(IO.Bind(m.Maybe, x => x.IfSuccess(out var v, out var e)
                                                                           ? IO.Return(SuccessOutcome(v))
                                                                           : handler(e).As().AsIo())),

            _ => throw new InvalidOperationException()
        };

    public static HK<OutcomeX<IO>, T> Catch<T>(HK<OutcomeX<IO>, T> ma, Func<ErrorInfo, T> handler) =>
        ma.As() switch {
            SuccessT<IO, T> s    => s,
            FailureT<IO, T> fail => new SuccessT<IO, T>(IO.Map(fail.ErrorInfo, handler)),
            MaybeT<IO, T> m      => new SuccessT<IO, T>(IO.Map(m.Maybe, x => x.Match(identity, handler))),

            _ => throw new InvalidOperationException()
        };

    public static HK<OutcomeX<IO>, B> BiMap<A, B>(HK<OutcomeX<IO>, A> ma, Func<A, B> mapSuccess, Func<ErrorInfo, ErrorInfo> mapFailure) {
        return ma.As() switch {
            SuccessT<IO, A> s    => new SuccessT<IO, B>(IO.Map(s.Value, mapSuccess)),
            FailureT<IO, A> fail => new FailureT<IO, B>(IO.Map(fail.ErrorInfo, mapFailure)),
            MaybeT<IO, A> m      => new MaybeT<IO, B>(IO.Map(m.Maybe, x => x.BiMap(mapSuccess, mapFailure))),

            _ => throw new InvalidOperationException()
        };
    }

    public static HK<OutcomeX<IO>, T> MapFailure<T>(HK<OutcomeX<IO>, T> ma, Func<ErrorInfo, ErrorInfo> map)
        => ma.As() switch {
            SuccessT<IO, T> s    => s,
            FailureT<IO, T> fail => new FailureT<IO, T>(IO.Map(fail.ErrorInfo, map)),
            MaybeT<IO, T> m      => new MaybeT<IO, T>(IO.Map(m.Maybe, x => x.Catch(map))),

            _ => throw new InvalidOperationException()
        };

    #endregion

    [Pure]
    public static HK<OutcomeX<IO>, B> Map<A, B>(HK<OutcomeX<IO>, A> ma, Func<A, B> f)
        => ma.As() switch {
            SuccessT<IO, A> s    => new SuccessT<IO, B>(IO.Map(s.Value, f)),
            FailureT<IO, A> fail => new FailureT<IO, B>(fail.ErrorInfo),
            MaybeT<IO, A> d      => new MaybeT<IO, B>(IO.Map(d.Maybe, x => x.Map(f).As())),

            _ => throw new InvalidOperationException()
        };

    #region Monad typeclass

    public static HK<OutcomeX<IO>, T> Return<T>(T value)
        => new SuccessT<IO, T>(IO.Return(value));

    public static HK<OutcomeX<IO>, B> Bind<A, B>(HK<OutcomeX<IO>, A> ma, Func<A, HK<OutcomeX<IO>, B>> f)
        => new MaybeT<IO, B>(ma.As() switch {
            // HK<IO,A> -> HK<IO, HK<OutcomeX<IO>, B>> -> HK<IO, OutcomeT<IO, B>> -> HK<IO, Outcome<B>>
            SuccessT<IO, A> s    => IO.Bind(s.Value, a => f(a).As().AsIo()),
            FailureT<IO, B> fail => IO.Map(fail.ErrorInfo, FailedOutcome<B>),

            // HK<IO, Outcome<A>> -map-> HK<IO, Outcome<HK<IO, Outcome<B>>> -> HK<IO, Outcome<B>>
            MaybeT<IO, A> d => IO.Bind(d.Maybe, x => x.Map(a => f(a).As().AsIo()).As().AsIo()),

            _ => throw new InvalidOperationException()
        });

    #endregion
}