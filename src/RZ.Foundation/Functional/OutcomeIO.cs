using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;
using RZ.Foundation.Types;

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

    public static OutcomeT<Asynchronous, C> SelectMany<A, B, C>(this OutcomeT<Synchronous, A> ma, Func<A, OutcomeT<Asynchronous, B>> bind,
                                                                Func<A, B, C> project) {
        return new MaybeT<Asynchronous, C>(new ConstantAsyncYield<Outcome<C>>(SyncToAsync()));

        async ValueTask<Outcome<C>> SyncToAsync() {
            if (ma.AsIo().RunIO().IfSuccess(out var a, out var e)){
                var ba = await bind(a).AsIo().RunIO();
                return ba.Map(b => project(a, b)).As();
            }
            else
                return e;
        }
    }

    public static OutcomeT<Asynchronous, C> SelectMany<A, B, C>(this OutcomeT<Asynchronous, A> ma, Func<A, OutcomeT<Synchronous, B>> bind,
                                                                Func<A, B, C> project) {
        return new MaybeT<Asynchronous, C>(new ConstantAsyncYield<Outcome<C>>(AsyncWithSync()));

        async ValueTask<Outcome<C>> AsyncWithSync() {
            var va = await ma.AsIo().RunIO();
            if (va.IfSuccess(out var a, out var e)){
                var ba = from b in bind(a)
                         select project(a, b);
                return ba.As().RunIO();
            }
            else
                return e;
        }
    }

    public static OutcomeT<Asynchronous, T> ToAsync<T>(this OutcomeT<Synchronous, T> ma) =>
        new MaybeT<Asynchronous, T>(Asynchronous.Return(ma.RunIO()));
}