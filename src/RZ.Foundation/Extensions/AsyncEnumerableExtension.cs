using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;

namespace RZ.Foundation.Extensions
{
    public static class AsyncEnumerableExtension
    {
        public static async IAsyncEnumerable<B> Aggregate<A, B>(this IAsyncEnumerable<A> source, Func<B, A, B> folder, B seed)
        {
            var accumulator = seed;
            await foreach (var item in source)
            {
                accumulator = folder(accumulator, item);
                yield return accumulator;
            }
        }

        public static async IAsyncEnumerable<B> AggregateAsync<A, B>(this IAsyncEnumerable<A> source, Func<B, A, Task<B>> folder, B seed)
        {
            var accumulator = seed;
            await foreach (var item in source)
            {
                accumulator = await folder(accumulator, item);
                yield return accumulator;
            }
        }

#pragma warning disable CS1998
        public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source) {
#pragma warning restore CS1998
            foreach (var item in source)
                yield return item;
        }

        public static async IAsyncEnumerable<B> MapAsync<A,B>(this IEnumerable<A> source, Func<A, Task<B>> selector) {
            foreach (var i in source) {
                var result = await selector(i);
                yield return result;
            }
        }

        public static async IAsyncEnumerable<B> MapAsync<A,B>(this IAsyncEnumerable<A> source, Func<A, Task<B>> selector) {
            await foreach (var i in source) {
                var result = await selector(i);
                yield return result;
            }
        }

        public static async IAsyncEnumerable<B> Map<A,B>(this IAsyncEnumerable<A> source, Func<A, B> selector) {
            await foreach (var i in source)
                yield return selector(i);
        }

        public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate) {
            await foreach (var i in source)
                if (predicate(i))
                    yield return i;
        }

        public static async IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> predicate) {
            await foreach (var i in source)
                if (await predicate(i))
                    yield return i;
        }

        public static async IAsyncEnumerable<B> ChainAsync<A, B>(this IEnumerable<A> source, Func<A, Task<IAsyncEnumerable<B>>> selector) {
            foreach (var i in source)
            await foreach (var inner in await selector(i))
                yield return inner;
        }

        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IEnumerable<A> source, Func<A, Task<Option<B>>> selector) {
            foreach (var i in source) {
                var result = await selector(i);
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async Task Iter<T>(this IAsyncEnumerable<T> source, Action<T> action) {
            await foreach (var i in source) action(i);
        }

        public static async Task Iter<T>(this IAsyncEnumerable<T> source, Func<T, Task> action) {
            await foreach (var i in source) await action(i);
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source) {
            var result = new List<T>();
            await source.Iter(result.Add);
            return result;
        }

        public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> source) {
            return (await source.ToListAsync()).ToArray();
        }
    }
}