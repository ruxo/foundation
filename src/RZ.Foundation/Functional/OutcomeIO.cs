using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt.Common;

namespace RZ.Foundation.Functional;

public static class OutcomeIO
{
    public static HK<IO, T> IfFail<IO, T>(this OutcomeT<IO, T> ma, T value) where IO : Functor<IO>, Monad<IO>, Eq<IO> =>
        IO.Map(ma.AsIo(), x => x.Match(identity, _ => value));

    public static HK<IO, Outcome<T>> IfFail<IO, T>(this OutcomeT<IO, T> ma, Action<Error> value)
        where IO : Functor<IO>, Monad<IO>, Eq<IO> =>
        IO.Map(ma.AsIo(), x => {
                              x.IfFail(value);
                              return x;
                          });

    public static HK<IO, T> IfFail<IO, T>(this OutcomeT<IO, T> ma, Func<Error, T> value) where IO : Functor<IO>, Monad<IO>, Eq<IO> =>
        IO.Map(ma.AsIo(), x => x.IfFail(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfFail<T>(this OutcomeT<Synchronous, T> ma, out Error error, out T value) =>
        ma.RunIO().IfFail(out error, out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IfSuccess<T>(this OutcomeT<Synchronous, T> ma, out T value, out Error error) =>
        ma.RunIO().IfSuccess(out value, out error);

    public static Outcome<T> RunIO<T>(this OutcomeT<Synchronous, T> ma) =>
        ma.AsIo().RunIO();

    public static ValueTask<Outcome<T>> RunIO<T>(this OutcomeT<Asynchronous, T> ma) =>
        ma.AsIo().RunIO();
}