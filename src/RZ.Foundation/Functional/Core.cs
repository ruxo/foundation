using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace RZ.Foundation.Functional;

public interface Eq<M> where M : Eq<M>
{
    public static abstract HK<M, bool> EqualsTo<T>(HK<M, T> a, HK<M, T> b);
    public static abstract HK<M, bool> NotEqualsTo<T>(HK<M, T> a, HK<M, T> b);
}

public static class MonadExtensions
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, B> Bind<M, A, B>(this HK<M, A> ma, Func<A, HK<M, B>> f) where M : Monad<M> =>
        M.Bind(ma, f);

    [Pure]
    public static HK<M, C> Bind<M, A, B, C>(this HK<M, A> ma, Func<A, HK<M, B>> bind, Func<A, B, C> project)
        where M : Monad<M>, Functor<M> =>
        M.Bind(ma, x => M.Map(bind(x), y => project(x, y)));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> Join<M, T>(this HK<M, HK<M, T>> ma) where M : Monad<M> =>
        M.Bind(ma, identity);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, C> SelectMany<M, A, B, C>(this HK<M, A> ma, Func<A, HK<M, B>> bind, Func<A, B, C> project)
        where M : Monad<M>, Functor<M> =>
        Bind(ma, bind, project);
}

public interface Monad<M> where M : Monad<M>
{
    public static abstract HK<M, T> Return<T>(T value);

    public static abstract HK<M, B> Bind<A, B>(HK<M, A> ma, Func<A, HK<M, B>> f);
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

public interface Functor<M> where M : Functor<M>
{
    public static abstract HK<M, B> Map<A, B>(HK<M, A> ma, Func<A, B> f);
}

/// <summary>
/// Higher-Kinded Type helper, as `K` in LanguageExt v5
/// </summary>
/// <typeparam name="M">Monad itself</typeparam>
/// <typeparam name="T">Type</typeparam>
public interface HK<M, T>;