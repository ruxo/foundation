// ReSharper disable CheckNamespace

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace RZ.Foundation;

public static class OutcomeExtension
{
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<B> Select<A, B>(this Outcome<A> ma, Func<A, B> map) =>
        ma.Map(map);

    [Pure]
    public static Outcome<C> SelectMany<A, B, C>(this Outcome<A> ma, Func<A, Outcome<B>> bind, Func<A, B, C> project)
        => ma.Bind(x => bind(x).Map(y => project(x, y)));
}