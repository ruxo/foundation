using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RZ.Foundation.Types;

namespace RZ.Foundation {
    public static class Prelude {
        public static Func<T> Constant<T>(T x) => () => x;
        public static T Identity<T>(T x) => x;
        public static void Noop() { }

        public static Func<T, T> SideEffect<T>(Action<T> f) => x => {
            f(x);
            return x;
        };

        /// <summary>
        /// Convert a value into Optional type. This one is inspired from LanguageExt lib :)
        /// </summary>
        /// <param name="val"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Option wrapped with the specific value</returns>
        public static Option<T> Optional<T>(T val) => val;

        public static Option<T> None<T>() => Option<T>.None();

        public static ApiResult<T> Success<T>(T val) => val;
        public static ApiResult<T> Failed<T>(Exception ex) => ex;

        public static TryAsync<T> TryAsync<T>(Func<Task<T>> runnable) => new TryAsync<T>(runnable);
        public static TryAsync<Unit> TryAsync(Func<Task> runnable) => new TryAsync<Unit>(async () => {
            await runnable();
            return Unit.Value;
        });

        public static TryCall<T> Try<T>(Func<T> runnable) => new TryCall<T>(runnable);
        public static TryCall<Unit> Try(Action action) => new TryCall<Unit>(() => { action(); return Unit.Value; });

        public static Iter<T> Iter<T>(IEnumerable<T> enumerable) => enumerable is Iter<T> iter ? iter : new Iter<T>(enumerable);

        public static Option<(A, B)> With<A, B>(Option<A> a, Option<B> b) => a.Chain(ax => b.Map(bx => (ax, bx)));
        public static Option<(A, B, C)> With<A, B, C>(Option<A> a, Option<B> b, Option<C> c) =>
            a.Chain(ax => b.Chain(bx => c.Map(cx => (ax, bx,cx))));

        public static ApiResult<(A, B)> With<A, B>(ApiResult<A> a, ApiResult<B> b) => a.Chain(ax => b.Map(bx => (ax, bx)));
        public static ApiResult<(A, B, C)> With<A, B, C>(ApiResult<A> a, ApiResult<B> b, ApiResult<C> c) =>
            a.Chain(ax => b.Chain(bx => c.Map(cx => (ax, bx,cx))));
    }

    public struct Unit
    {
        public static readonly Unit Value = new Unit();
        public override bool Equals(object obj) => obj is Unit;
        public override int GetHashCode() => 0;
    }
}
