using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RZ.Foundation.Types;
// ReSharper disable InconsistentNaming

namespace RZ.Foundation.Functional;

public static class OutcomeIO
{
    public static OutcomeT<Asynchronous, T> IfFail<T>(this OutcomeT<Asynchronous, T> ma, Func<ErrorInfo, Task<Unit>> fail) =>
        new MaybeT<Asynchronous, T>(ma.AsIo().Bind(x => x.IfSuccess(out _, out var e)
                                                            ? Asynchronous.Return(x)
                                                            : new FunctionAsyncYield<Outcome<T>>(async () => {
                                                                await fail(e);
                                                                return FailedOutcome<T>(e);
                                                            })));

    public static HK<IO, T> IfFail<IO, T>(this OutcomeT<IO, T> ma, T value)
        where IO : Functor<IO>, Monad<IO>, Eq<IO>
        =>
            IO.Map(ma.AsIo(), x => x.Match(identity, _ => value));

    public static HK<IO, Outcome<T>> IfFail<IO, T>(this OutcomeT<IO, T> ma, Action<ErrorInfo> value)
        where IO : Functor<IO>, Monad<IO>, Eq<IO>
        =>
            IO.Map(ma.AsIo(), x => {
                x.IfFail(value);
                return x;
            });

    public static HK<IO, T> IfFail<IO, T>(this OutcomeT<IO, T> ma, Func<ErrorInfo, T> value)
        where IO : Functor<IO>, Monad<IO>, Eq<IO>
        =>
            IO.Map(ma.AsIo(), x => x.IfFail(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfFail<T>(this OutcomeT<Synchronous, T> ma, out ErrorInfo error, out T value) =>
        ma.RunIO().IfFail(out error, out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfSuccess<T>(this OutcomeT<Synchronous, T> ma, out T value, out ErrorInfo error) =>
        ma.RunIO().IfSuccess(out value, out error);

    public static Outcome<T> RunIO<T>(this OutcomeT<Synchronous, T> ma) =>
        ma.AsIo().RunIO();

    public static ValueTask<Outcome<T>> RunIO<T>(this OutcomeT<Asynchronous, T> ma) =>
        ma.AsIo().RunIO();

    #region LINQ sync to async

    public static HK<OutcomeX<Asynchronous>, C> SelectMany<A, B, C>(this HK<OutcomeX<Synchronous>, A> ma,
                                                                    Func<A, HK<OutcomeX<Asynchronous>, B>> bind, Func<A, B, C> project) {
        return new MaybeT<Asynchronous, C>(new ConstantAsyncYield<Outcome<C>>(SyncToAsync()));

        async ValueTask<Outcome<C>> SyncToAsync() {
            if (ma.As().RunIO().IfSuccess(out var a, out var e)){
                var ba = await bind(a).As().AsIo().RunIO();
                return ba.Map(b => project(a, b)).As();
            }
            else
                return e;
        }
    }

    public static HK<OutcomeX<Asynchronous>, C> SelectMany<A, B, C>(this HK<OutcomeX<Asynchronous>, A> ma,
                                                                Func<A, HK<OutcomeX<Synchronous>, B>> bind,
                                                                Func<A, B, C> project) {
        return new MaybeT<Asynchronous, C>(new ConstantAsyncYield<Outcome<C>>(AsyncWithSync()));

        async ValueTask<Outcome<C>> AsyncWithSync() {
            var va = await ma.As().RunIO();
            if (va.IfSuccess(out var a, out var e)){
                var ba = from b in bind(a)
                         select project(a, b);
                return ba.As().RunIO();
            }
            else
                return e;
        }
    }

    public static OutcomeT<Asynchronous, C> SelectMany<A, B, C>(this OutcomeT<Synchronous, A> ma,
                                                                Func<A, OutcomeT<Asynchronous, B>> bind,
                                                                Func<A, B, C> project)
        => ((HK<OutcomeX<Synchronous>, A>)ma).SelectMany(bind, project).As();

    public static OutcomeT<Asynchronous, C> SelectMany<A, B, C>(this OutcomeT<Asynchronous, A> ma,
                                                                Func<A, OutcomeT<Synchronous, B>> bind,
                                                                Func<A, B, C> project)
        => ((HK<OutcomeX<Asynchronous>, A>)ma).SelectMany(bind, project).As();

    #endregion

    public static HK<OutcomeX<Asynchronous>, B> SelectMany<A, B>(this HK<OutcomeX<Asynchronous>, A> ma,
                                                                 Func<A, Guard<ErrorInfo>> bind, Func<A, A, B> project)
        => from a in ma
           let b = bind(a)
           from result in b.Flag ? SuccessAsync(a) : FailureAsync<A>(b.OnFalse())
           select project(a, result);

    public static HK<OutcomeX<Synchronous>, B> SelectMany<A, B>(this HK<OutcomeX<Synchronous>, A> ma,
                                                                Func<A, Guard<ErrorInfo>> bind, Func<A, A, B> project)
        => from a in ma
           let b = bind(a)
           from result in b.Flag ? Success(a) : Failure<A>(b.OnFalse())
           select project(a, result);

    public static OutcomeT<Asynchronous, T> ToAsync<T>(this OutcomeT<Synchronous, T> ma) =>
        new MaybeT<Asynchronous, T>(Asynchronous.Return(ma.RunIO()));
}