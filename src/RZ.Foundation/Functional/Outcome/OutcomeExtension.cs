// ReSharper disable CheckNamespace

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LanguageExt;
using LanguageExt.Common;
using RZ.Foundation.Functional;

namespace RZ.Foundation;

public static partial class OutcomeExtension
{
    [Pure]
    public static Outcome<B> Bind<A,B>(this Outcome<A> ma, Func<A, Outcome<B>> bind) =>
        ma.Either.Bind(a => bind(a).Either);

    [Pure]
    public static OutcomeAsync<B> Bind<A,B>(this Outcome<A> ma, Func<A, OutcomeAsync<B>> bind) =>
        ma.Either.ToAsync().Bind(a => bind(a).Either);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<B> Map<A,B>(this Outcome<A> ma, Func<A, B> map) =>
        ma.Either.Map(map);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<A> MapFailure<A>(this Outcome<A> ma, Func<Error, Error> map) =>
        ma.Either.MapLeft(map);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<B> Select<A,B>(this Outcome<A> ma, Func<A, B> map) =>
        Map(ma, map);

    [Pure]
    public static Outcome<C> SelectMany<A, B, C>(this Outcome<A> ma, Func<A, Outcome<B>> bind, Func<A, B, C> project) =>
        Bind(ma, x => Map(bind(x), y => project(x,y)));

    [Pure]
    public static OutcomeAsync<C> SelectMany<A, B, C>(this Outcome<A> ma, Func<A, OutcomeAsync<B>> bind, Func<A, B, C> project) =>
        Bind(ma, x => Map(bind(x), y => project(x,y)));

    [Pure]
    public static Outcome<T> ToOutcome<T>(this Option<T> opt, Error? error = default) =>
        opt.Match(v => (Outcome<T>)v, () => error ?? StandardErrors.NotFound);

    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Outcome<T> ToOutcome<T>(this Try<T> self) => self.ToEither(Error.New);
}