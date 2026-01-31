// ReSharper disable CheckNamespace

using System.Runtime.CompilerServices;

namespace RZ.Foundation;

[PublicAPI]
public static class OutcomeExtension
{
    [Pure]
    public static async ValueTask<Outcome<C>> SelectMany<A, B, C>(this Outcome<A> ma, Func<A, ValueTask<Outcome<B>>> bind, Func<A, B, C> project) {
        if (ma.IsFail) return ma.Error!;
        if (Fail(await bind(ma.Data!).ConfigureAwait(false), out var e, out var result)) return e;
        return project(ma.Data!, result);
    }

    extension<A>(in Outcome<A> ma)
    {
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Outcome<B> Select<B>(Func<A, B> map) =>
            ma.Map(map);

        [Pure]
        public Outcome<C> SelectMany<B, C>(Func<A, Outcome<B>> bind, Func<A, B, C> project)
            => ma.Bind(x => bind(x).Map(y => project(x, y)));
    }

    extension<A>(ValueTask<Outcome<A>> ma)
    {
        [Pure]
        public async ValueTask<Outcome<B>> Select<B>(Func<A, B> map)
            => Fail(await ma.ConfigureAwait(false), out var e, out var a) ? e : map(a);

        [Pure]
        public async ValueTask<Outcome<C>> SelectMany<B, C>(Func<A, Outcome<B>> bind, Func<A, B, C> project) {
            if (Fail(await ma.ConfigureAwait(false), out var e, out var a)
             || Fail(bind(a), out e, out var result)) return e;
            return project(a, result);
        }

        [Pure]
        public async ValueTask<Outcome<C>> SelectMany<B, C>(Func<A, ValueTask<Outcome<B>>> bind, Func<A, B, C> project) {
            if (Fail(await ma.ConfigureAwait(false), out var e, out var a)
             || Fail(await bind(a), out e, out var result)) return e;
            return project(a, result);
        }

        [Pure, PublicAPI]
        public async ValueTask<Outcome<Option<A>>> CheckNotFound() {
            if (Fail(await ma.ConfigureAwait(false), out var e, out var a))
                return e.IsNotFound() ? SuccessOutcome<Option<A>>(None) : e;
            return SuccessOutcome(Some(a));
        }
    }
}