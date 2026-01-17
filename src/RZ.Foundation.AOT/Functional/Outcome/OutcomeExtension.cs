// ReSharper disable CheckNamespace

using System.Runtime.CompilerServices;

namespace RZ.Foundation;

[PublicAPI]
public static class OutcomeExtension
{
    extension<A>(Outcome<A> ma)
    {
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Outcome<B> Select<B>(Func<A, B> map) =>
            ma.Map(map);

        [Pure]
        public Outcome<C> SelectMany<B, C>(Func<A, Outcome<B>> bind, Func<A, B, C> project)
            => ma.Bind(x => bind(x).Map(y => project(x, y)));
    }
}