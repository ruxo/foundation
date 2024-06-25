using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace RZ.Foundation.Functional;

public interface Functor<M> where M : Functor<M>
{
    public static abstract HK<M, B> Map<A, B>(HK<M, A> ma, Func<A, B> f);
}

public static class FunctorExtensions
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, B> Map<M, A, B>(this HK<M, A> ma, Func<A, B> f) where M : Functor<M> =>
        M.Map(ma, f);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, B> Select<M, A, B>(this HK<M, A> ma, Func<A, B> f) where M : Functor<M> =>
        M.Map(ma, f);
}
