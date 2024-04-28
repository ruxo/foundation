// ReSharper disable CheckNamespace

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace RZ.Foundation;

public static partial class OutcomeExtension
{
    [Pure]
    public static Outcome<B> Bind<A,B>(this Outcome<A> ma, Func<A, Outcome<B>> bind) =>
        ma.Either.Bind(a => bind(a).Either);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<B> Map<A,B>(this Outcome<A> ma, Func<A, B> map) =>
        ma.Either.Map(map);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<B> Select<A,B>(this Outcome<A> ma, Func<A, B> map) =>
        Map(ma, map);

    [Pure]
    public static Outcome<C> SelectMany<A, B, C>(this Outcome<A> ma, Func<A, Outcome<B>> bind, Func<A, B, C> project) =>
        Bind(ma, x => Map(bind(x), y => project(x,y)));
}