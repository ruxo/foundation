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

        #region Map methods
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
        #endregion

        #region Flatten methods

        public static async IAsyncEnumerable<B> FlattenT<A,B>(this IEnumerable<A> source, Func<A, Task<IEnumerable<B>>> selector) {
            foreach (var i in source)
            foreach (var inner in await selector(i))
                yield return inner;
        }

        public static async IAsyncEnumerable<B> FlattenAsync<A,B>(this IAsyncEnumerable<A> source, Func<A, Task<IEnumerable<B>>> selector) {
            await foreach (var i in source)
            foreach (var inner in await selector(i))
                yield return inner;
        }

        public static async IAsyncEnumerable<B> Flatten<A,B>(this IAsyncEnumerable<A> source, Func<A, IEnumerable<B>> selector) {
            await foreach (var i in source)
            foreach (var inner in selector(i))
                yield return inner;
        }

        public static async IAsyncEnumerable<B> FlattenT<A,B>(this IEnumerable<A> source, Func<A, IAsyncEnumerable<B>> selector) {
            foreach (var i in source)
            await foreach (var inner in selector(i))
                yield return inner;
        }

        public static async IAsyncEnumerable<B> Flatten<A,B>(this IAsyncEnumerable<A> source, Func<A, IAsyncEnumerable<B>> selector) {
            await foreach (var i in source)
            await foreach (var inner in selector(i))
                yield return inner;
        }

        #endregion

        #region Where methods
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
        #endregion

        public static async Task<bool> All<T>(this IAsyncEnumerable<T> source, Predicate<T> predicate) {
            await foreach(var i in source)
                if (!predicate(i))
                    return false;
            return true;
        }

        public static async Task<bool> Any<T>(this IAsyncEnumerable<T> source, Predicate<T> predicate) {
            await foreach(var i in source)
                if (predicate(i))
                    return true;
            return false;
        }

        public static async Task<bool> Any<T>(this IAsyncEnumerable<T> source) {
            await using var itor = source.GetAsyncEnumerator();
            return await itor.MoveNextAsync();
        }

        public static async IAsyncEnumerable<T> Append<T>(this IAsyncEnumerable<T> source, T element) {
            await foreach (var i in source)
                yield return i;
            yield return element;
        }

        public static async IAsyncEnumerable<T> AppendAsync<T>(this IAsyncEnumerable<T> source, Task<T> element) {
            await foreach (var i in source)
                yield return i;
            yield return await element;
        }

        public static async IAsyncEnumerable<T> AppendAsync<T>(this IAsyncEnumerable<T> source, ValueTask<T> element) {
            await foreach (var i in source)
                yield return i;
            yield return await element;
        }
        
        public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> first, IEnumerable<T> second) {
            await foreach (var i in first)
                yield return i;
            foreach (var i in second)
                yield return i;
        }
        
        public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> first, IAsyncEnumerable<T> second) {
            await foreach (var i in first)
                yield return i;
            await foreach (var i in second)
                yield return i;
        }

        #region Choose methods
        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IEnumerable<A> source, Func<A, Task<Option<B>>> selector) {
            foreach (var i in source) {
                var result = await selector(i);
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IEnumerable<A> source, Func<A, ValueTask<Option<B>>> selector) {
            foreach (var i in source) {
                var result = await selector(i);
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IEnumerable<A> source, Func<A, OptionAsync<B>> selector) {
            foreach (var i in source) {
                var result = await selector(i).ToOption();
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IAsyncEnumerable<A> source, Func<A, Task<Option<B>>> selector) {
            await foreach (var i in source) {
                var result = await selector(i);
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IAsyncEnumerable<A> source, Func<A, ValueTask<Option<B>>> selector) {
            await foreach (var i in source) {
                var result = await selector(i);
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IAsyncEnumerable<A> source, Func<A, OptionAsync<B>> selector) {
            await foreach (var i in source) {
                var result = await selector(i).ToOption();
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async IAsyncEnumerable<B> Choose<A, B>(this IAsyncEnumerable<A> source, Func<A, Option<B>> selector) {
            await foreach (var i in source) {
                var result = selector(i);
                if (result.IsSome)
                    yield return result.Get();
            }
        }
        #endregion

        #region Iter methods
        public static async Task Iter<T>(this IAsyncEnumerable<T> source, Action<T> action) {
            await foreach (var i in source) action(i);
        }

        public static async Task Iter<T>(this IAsyncEnumerable<T> source, Func<T, Task> action) {
            await foreach (var i in source) await action(i);
        }
        #endregion

        #region First

        public static async Task<Option<T>> TryFirst<T>(this IAsyncEnumerable<T> source) {
            await using var enumerator = source.GetAsyncEnumerator();
            return await enumerator.MoveNextAsync() ? enumerator.Current : LanguageExt.Prelude.None;
        }

        public static async Task<Option<T>> TryFirst<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate) {
            await using var enumerator = source.GetAsyncEnumerator();
            while(await enumerator.MoveNextAsync())
                if (predicate(enumerator.Current))
                    return enumerator.Current;
            return LanguageExt.Prelude.None;
        }

        public static async Task<Option<T>> TryFirstAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> predicate) {
            await using var enumerator = source.GetAsyncEnumerator();
            while(await enumerator.MoveNextAsync())
                if (await predicate(enumerator.Current))
                    return enumerator.Current;
            return LanguageExt.Prelude.None;
        }

        #endregion
        
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