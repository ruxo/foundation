using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using RZ.Foundation.Types;

namespace RZ.Foundation {
    public static class Prelude {
        [Obsolete("use constant instead")]
        public static Func<T> Constant<T>(T x) => constant(x);

        public static Func<T> constant<T>(T x) => () => x;

        [Obsolete("use identity instead")]
        public static T Identity<T>(T x) => x;
        public static void Noop() { }

        public static Func<T, T> SideEffect<T>(Action<T> f) => x => {

            f(x);
            return x;
        };

        public static Func<T, Task<T>> SideEffectAsync<T>(Func<T, Task> f) => async x => {
            await f(x);
            return x;
        };

        public static T SideEffect<T>(this T x, Action<T> f) {
            f(x);
            return x;
        }

        public static async Task<T> SideEffectAsync<T>(this T x, Func<T,Task> f) {
            await f(x);
            return x;
        }

        public static ApiResult<T> Success<T>(T val) => val;
        public static ApiResult<T> Failed<T>(Exception ex) => ex;

        public static Iter<T> Iter<T>(IEnumerable<T> enumerable) => enumerable is Iter<T> iter ? iter : new Iter<T>(enumerable);

        public static Option<(A, B)> With<A, B>(Option<A> a, Option<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));
        public static Option<(A, B, C)> With<A, B, C>(Option<A> a, Option<B> b, Option<C> c) =>
            a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx,cx))));

        public static ApiResult<(A, B)> With<A, B>(ApiResult<A> a, ApiResult<B> b) => a.Chain(ax => b.Map(bx => (ax, bx)));
        public static ApiResult<(A, B, C)> With<A, B, C>(ApiResult<A> a, ApiResult<B> b, ApiResult<C> c) =>
            a.Chain(ax => b.Chain(bx => c.Map(cx => (ax, bx,cx))));
    }
}
