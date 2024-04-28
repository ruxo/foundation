using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LanguageExt.Common;

namespace RZ.Foundation.Functional;

public static class ErrorHandlerableExtensions
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> Catch<M, T>(this HK<M, T> ma, Func<Error, Error> handler) where M : ErrorHandlerable<M> =>
        M.MapFailure(ma, handler);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> Catch<M, T>(this HK<M, T> ma, Func<Error, T> handler) where M : ErrorHandlerable<M> =>
        M.Catch(ma, handler);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> Catch<M, T>(this HK<M, T> ma, Func<Error, HK<M, T>> handler) where M : ErrorHandlerable<M> =>
        M.Catch(ma, handler);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HK<M, T> MapFailure<M, T>(this HK<M, T> ma, Func<Error, Error> map) where M : ErrorHandlerable<M> =>
        M.MapFailure(ma, map);
}

public interface ErrorHandlerable<M> where M : ErrorHandlerable<M>
{
    public static abstract HK<M, T> Catch<T>(HK<M, T> ma, Func<Error, T> handler);

    public static abstract HK<M, T> Catch<T>(HK<M, T> ma, Func<Error, HK<M, T>> handler);

    public static abstract HK<M, T> MapFailure<T>(HK<M, T> ma, Func<Error, Error> map);
}