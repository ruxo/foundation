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

        [Pure]
        public async ValueTask<Outcome<C>> SelectMany<B, C>(Func<A, ValueTask<Outcome<B>>> bind, Func<A, B, C> project) {
            if (ma.IsFail) return ma.Error!;
            if (Fail(await bind(ma.Data!), out var e, out var result)) return e;
            return project(ma.Data!, result);
        }
    }

    extension<A>(ValueTask<Outcome<A>> ma)
    {
        [Pure]
        public async ValueTask<Outcome<C>> SelectMany<B, C>(Func<A, Outcome<B>> bind, Func<A, B, C> project) {
            if (Fail(await ma, out var e, out var a)
             || Fail(bind(a), out e, out var result)) return e;
            return project(a, result);
        }

        [Pure]
        public async ValueTask<Outcome<C>> SelectMany<B, C>(Func<A, ValueTask<Outcome<B>>> bind, Func<A, B, C> project) {
            if (Fail(await ma, out var e, out var a)
             || Fail(await bind(a), out e, out var result)) return e;
            return project(a, result);
        }
    }
}