// ReSharper disable CheckNamespace

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LanguageExt;

namespace RZ.Foundation;

public static partial class OutcomeExtension
{
    [Pure]
    public static OutcomeAsync<B> Bind<A, B>(this OutcomeAsync<A> ma, Func<A, Outcome<B>> bind) =>
        ma.Either.Bind(a => bind(a).Either.ToAsync());

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsync<B> Map<A, B>(this OutcomeAsync<A> ma, Func<A, B> map) => ma.Map(map);

    [Pure]
    public static OutcomeAsync<C> SelectMany<A, B, C>(this OutcomeAsync<A> ma, Func<A, Outcome<B>> bind, Func<A, B, C> project) =>
        Bind(ma, x => Map(bind(x), y => project(x, y)));

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OutcomeAsync<T> ToOutcome<T>(this TryAsync<T> self) => self.ToEither();
}