using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using RZ.Foundation.Types;

namespace RZ.Foundation.Functional;

public static class ErrorHandlerableExtensions
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> Catch<M, T>(this HK<M, T> ma, Func<ErrorInfo, ErrorInfo> handler)
        where M : ErrorHandlerable<M> =>
        M.MapFailure(ma, handler);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> Catch<M, T>(this HK<M, T> ma, Func<ErrorInfo, T> handler)
        where M : ErrorHandlerable<M> =>
        M.Catch(ma, handler);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> Catch<M, T>(this HK<M, T> ma, Func<ErrorInfo, HK<M, T>> handler)
        where M : ErrorHandlerable<M> =>
        M.Catch(ma, handler);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> MapFailure<M, T>(this HK<M, T> ma, Func<ErrorInfo, ErrorInfo> map)
        where M : ErrorHandlerable<M> =>
        M.MapFailure(ma, map);
}

public interface ErrorHandlerable<M> where M : ErrorHandlerable<M>
{
    public static abstract HK<M, T> Catch<T>(HK<M, T> ma, Func<ErrorInfo, T> handler);

    public static abstract HK<M, T> Catch<T>(HK<M, T> ma, Func<ErrorInfo, HK<M, T>> handler);

    [Pure]
    public static abstract HK<M, B> BiMap<A,B>(HK<M, A> ma, Func<A,B> mapSuccess, Func<ErrorInfo, ErrorInfo> mapFailure);

    [Pure]
    public static abstract HK<M, T> MapFailure<T>(HK<M, T> ma, Func<ErrorInfo, ErrorInfo> map);
}