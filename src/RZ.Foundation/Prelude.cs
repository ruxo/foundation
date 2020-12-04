using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using RZ.Foundation.Extensions;

namespace RZ.Foundation {
    public static class Prelude {
        public static Func<T> constant<T>(T x) => () => x;

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

        public static Result<T> Success<T>(T val) => val;
        public static Result<T> Failed<T>(Exception ex) => new Result<T>(ex);

        public static Option<(A, B)> With<A, B>(Option<A> a, Option<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));
        public static Option<(A, B, C)> With<A, B, C>(Option<A> a, Option<B> b, Option<C> c) =>
            a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx,cx))));

        public static Result<(A, B)> With<A, B>(Result<A> a, Result<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));
        public static Result<(A, B, C)> With<A, B, C>(Result<A> a, Result<B> b, Result<C> c) =>
            a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx,cx))));
    }
}
