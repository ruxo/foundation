using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using static LanguageExt.Prelude;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RZ.Foundation.Extensions
{
    public static class AsyncEnumerableExtension
    {
        #region Aggregation

        public static async IAsyncEnumerable<B> Aggregate<A, B>(this IAsyncEnumerable<A> source, Func<B, A, B> folder,
            B seed,
            [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var accumulator = seed;
            await foreach (var item in source.WithCancellation(cancelToken))
            {
                accumulator = folder(accumulator, item);
                yield return accumulator;
            }
        }

        public static async IAsyncEnumerable<B> AggregateAsync<A, B>(this IAsyncEnumerable<A> source,
            Func<B, A, Task<B>> folder, B seed,
            [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var accumulator = seed;
            await foreach (var item in source.WithCancellation(cancelToken))
            {
                accumulator = await folder(accumulator, item);
                yield return accumulator;
            }
        }

        #endregion

        public static async ValueTask<bool> All<T>(this IAsyncEnumerable<T> source, Func<T,bool> predicate,
            CancellationToken cancelToken = default) =>
            (await source.TryFind(i => !predicate(i), cancelToken)).IsNone;

        public static async IAsyncEnumerable<(T, T)> AllPairs<T>(this IAsyncEnumerable<T> a, IAsyncEnumerable<T> b,
            [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await using var aItor = a.GetAsyncEnumerator(cancelToken);
            await using var bItor = b.GetAsyncEnumerator(cancelToken);
            while (await aItor.MoveNextAsync() && await bItor.MoveNextAsync())
            {
                yield return (aItor.Current, bItor.Current);
            }
        }

        public static async ValueTask<bool> Any<T>(this IAsyncEnumerable<T> source, Func<T,bool> predicate,
            CancellationToken cancelToken = default) =>
            (await source.TryFind(predicate, cancelToken)).IsSome;

        public static async ValueTask<bool> Any<T>(this IAsyncEnumerable<T> source,
            CancellationToken cancelToken = default)
        {
            await using var itor = source.GetAsyncEnumerator(cancelToken);
            return await itor.MoveNextAsync();
        }

        #region Append

        public static async IAsyncEnumerable<T> Append<T>(this IAsyncEnumerable<T> source, T element,
            [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var i in source.WithCancellation(cancelToken))
                yield return i;
            yield return element;
        }

        public static async IAsyncEnumerable<T> AppendAsync<T>(this IAsyncEnumerable<T> source, Task<T> element,
            [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var i in source.WithCancellation(cancelToken))
                yield return i;
            yield return await element;
        }

        public static async IAsyncEnumerable<T> AppendAsync<T>(this IAsyncEnumerable<T> source, ValueTask<T> element,
            [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await foreach (var i in source.WithCancellation(cancelToken))
                yield return i;
            yield return await element;
        }

        #endregion

#pragma warning disable CS1998
        public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source)
        {
#pragma warning restore CS1998
            foreach (var item in source)
                yield return item;
        }

        #region Average methods

        public static ValueTask<T> Average<T>(this IAsyncEnumerable<T> seq, CancellationToken cancelToken = default) where T: INumber<T> => 
            seq.AverageBy(identity, cancelToken);

        public static async ValueTask<TAverage> AverageBy<T, TAverage>(this IAsyncEnumerable<T> seq, Func<T, TAverage> getter, CancellationToken cancelToken = default)
            where TAverage : INumber<TAverage> {
            var v = TAverage.Zero;
            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken)) {
                v += getter(i);
                counter++;
            }
            return v / TAverage.CreateChecked(counter);
        }

        public static async ValueTask<Option<TAverage>> TryAverageBy<T, TAverage>(this IAsyncEnumerable<T> seq, Func<T, TAverage> getter, CancellationToken cancelToken = default)
            where TAverage : INumber<TAverage> {
            var v = TAverage.Zero;
            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken)) {
                v += getter(i);
                counter++;
            }
            return counter == 0? None : Some(v / TAverage.CreateChecked(counter));
        }

        #endregion

        public static IAsyncEnumerable<R> Cast<T, R>(this IAsyncEnumerable<T> seq, CancellationToken cancelToken = default) where T: class where R: class =>
            seq.Map(x => (R)(object) x, cancelToken);

        #region Choose methods
        public static IAsyncEnumerable<B> ChooseAsync<A, B>(this IEnumerable<A> source, Func<A, Task<Option<B>>> selector) => 
            source.ChooseAsync(x => selector(x).ToValue());

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

        public static IAsyncEnumerable<B> ChooseAsync<A, B>(this IAsyncEnumerable<A> source, Func<A, Task<Option<B>>> selector, CancellationToken cancelToken = default) => 
            source.ChooseAsync(x => selector(x).ToValue(), cancelToken);

        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IAsyncEnumerable<A> source, Func<A, ValueTask<Option<B>>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken)) {
                var result = await selector(i);
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async IAsyncEnumerable<B> ChooseAsync<A, B>(this IAsyncEnumerable<A> source, Func<A, OptionAsync<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken)) {
                var result = await selector(i).ToOption();
                if (result.IsSome)
                    yield return result.Get();
            }
        }

        public static async IAsyncEnumerable<B> Choose<A, B>(this IAsyncEnumerable<A> source, Func<A, Option<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken)) {
                var result = selector(i);
                if (result.IsSome)
                    yield return result.Get();
            }
        }
        #endregion

        public static async IAsyncEnumerable<Seq<T>> Chunk<T>(this IAsyncEnumerable<T> seq, int size, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            while (true) {
                var result = await itor.TakeAtMost(size);
                if (result.IsEmpty) break;
                yield return result;
            }
        }

        public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> first, IEnumerable<T> second, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in first.WithCancellation(cancelToken))
                yield return i;
            foreach (var i in second)
                yield return i;
        }
        
        public static async IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> first, IAsyncEnumerable<T> second, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in first.WithCancellation(cancelToken))
                yield return i;
            await foreach (var i in second.WithCancellation(cancelToken))
                yield return i;
        }

        public static async ValueTask<bool> Contains<T>(this IAsyncEnumerable<T> seq, T value, CancellationToken cancelToken = default) where T: IEqualityOperators<T,T,bool> {
            await foreach (var i in seq.WithCancellation(cancelToken))
                if (i == value)
                    return true;
            return false;
        }

        public static async ValueTask<int> Count<T>(this IAsyncEnumerable<T> seq, CancellationToken cancelToken = default)
        {
            var counter = 0;
            await foreach (var _ in seq.WithCancellation(cancelToken))
                ++counter;
            return counter;
        }

        public static ValueTask<int> Count<T>(this IAsyncEnumerable<T> seq, Func<T,bool> predicate, CancellationToken cancelToken = default) =>
            seq.Where(predicate, cancelToken).Count(cancelToken);

        public static async ValueTask<int> CountBy<T, R>(this IAsyncEnumerable<T> seq, Func<T, R> keyGetter, CancellationToken cancelToken = default)
        {
            var set = new System.Collections.Generic.HashSet<R>(16);
            await foreach (var i in seq.WithCancellation(cancelToken))
            {
                var key = keyGetter(i);
                if (set.Contains(key)) continue;
                set.Add(key);
            }
            return set.Count;
        }

        public static IAsyncEnumerable<T> Distinct<T>(this IAsyncEnumerable<T> seq, CancellationToken cancelToken = default) => 
            seq.DistinctBy(identity, cancelToken: cancelToken);

        public static async IAsyncEnumerable<T> DistinctBy<T,R>(this IAsyncEnumerable<T> seq, Func<T,R> keyGetter, IEqualityComparer<R>? comparer = null, [EnumeratorCancellation] CancellationToken cancelToken = default) 
        {
            var set = new System.Collections.Generic.HashSet<R>(16, comparer);
            await foreach (var i in seq.WithCancellation(cancelToken))
            {
                var key = keyGetter(i);
                if (set.Contains(key)) continue;
                set.Add(key);
                yield return i;
            }
        }

#pragma warning disable CS1998
        public static async IAsyncEnumerable<T> Empty<T>()
        {
            yield break;
        }
#pragma warning restore CS1998

        public static IAsyncEnumerable<T> Except<T>(this IAsyncEnumerable<T> seq, IAsyncEnumerable<T> another,
            IEqualityComparer<T>? comparer = null, CancellationToken cancelToken = default) =>
            seq.ExceptBy(another, identity, comparer, cancelToken);

        public static async IAsyncEnumerable<T> ExceptBy<T,K>(this IAsyncEnumerable<T> seq, IAsyncEnumerable<T> another, Func<T,K> keyGetter, IEqualityComparer<K>? comparer = null, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var set = new System.Collections.Generic.HashSet<K>(await another.Map(keyGetter,cancelToken).ToListAsync(cancelToken), comparer);
            await foreach (var i in seq.WithCancellation(cancelToken))
            {
                var key = keyGetter(i);
                if (!set.Contains(key))
                    yield return i;
            }
        }

        #region Flatten methods

        public static async IAsyncEnumerable<B> FlattenT<A,B>(this IEnumerable<A> source, Func<A, Task<IEnumerable<B>>> selector) {
            foreach (var i in source)
            foreach (var inner in await selector(i))
                yield return inner;
        }

        public static IAsyncEnumerable<B> FlattenAsync<A, B>(this IAsyncEnumerable<A> source,
            Func<A, Task<IEnumerable<B>>> selector, CancellationToken cancelToken = default) =>
            source.FlattenAsync(x => selector(x).ToValue(), cancelToken);

        public static async IAsyncEnumerable<B> FlattenAsync<A,B>(this IAsyncEnumerable<A> source, Func<A, ValueTask<IEnumerable<B>>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken))
            foreach (var inner in await selector(i))
                yield return inner;
        }

        public static async IAsyncEnumerable<B> Flatten<A,B>(this IAsyncEnumerable<A> source, Func<A, IEnumerable<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken))
            foreach (var inner in selector(i))
                yield return inner;
        }

        public static async IAsyncEnumerable<B> FlattenT<A,B>(this IEnumerable<A> source, Func<A, IAsyncEnumerable<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            foreach (var i in source)
            await foreach (var inner in selector(i).WithCancellation(cancelToken))
                yield return inner;
        }

        public static async IAsyncEnumerable<B> Flatten<A,B>(this IAsyncEnumerable<A> source, Func<A, IAsyncEnumerable<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken))
            await foreach (var inner in selector(i))
                yield return inner;
        }

        #endregion

        #region Map methods
        public static async IAsyncEnumerable<B> MapAsync<A,B>(this IEnumerable<A> source, Func<A, Task<B>> selector) {
            foreach (var i in source) {
                var result = await selector(i);
                yield return result;
            }
        }

        public static async IAsyncEnumerable<B> MapAsync<A,B>(this IAsyncEnumerable<A> source, Func<A, Task<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken)) {
                var result = await selector(i);
                yield return result;
            }
        }

        public static async IAsyncEnumerable<B> Map<A,B>(this IAsyncEnumerable<A> source, Func<A, B> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken))
                yield return selector(i);
        }
        #endregion

        #region Iter methods
        public static async Task Iter<T>(this IAsyncEnumerable<T> source, Action<T> action, CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken)) action(i);
        }

        public static async Task Iter<T>(this IAsyncEnumerable<T> source, Func<T, Task> action, CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken)) await action(i);
        }
        #endregion

        public static IAsyncEnumerable<R> OfType<T, R>(this IAsyncEnumerable<T> seq, CancellationToken cancelToken = default) =>
            seq.Choose(x => x is R v ? Some(v) : None, cancelToken);

        public static async IAsyncEnumerable<T> Prepend<T>(this IAsyncEnumerable<T> seq, T value, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            yield return value;
            await foreach (var i in seq.WithCancellation(cancelToken))
                yield return i;
        }

        public static async IAsyncEnumerable<T> Reverse<T>(this IAsyncEnumerable<T> seq, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            var stack = new Stack<T>(16);
            await seq.Iter(i => stack.Push(i), cancelToken);
            while(stack.TryPop(out var i))
                yield return i;
        }
       
        public static IAsyncEnumerable<T> Skip<T>(this IAsyncEnumerable<T> seq, int n, CancellationToken cancelToken = default)
        {
            var counter = 0;
            return seq.SkipWhile(_ => counter++ < n, cancelToken);
        }

        public static async IAsyncEnumerable<T> SkipWhile<T>(this IAsyncEnumerable<T> seq, Func<T, bool> predicate, [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            if (!await itor.MoveNextAsync()) yield break;

            var skipping = predicate(itor.Current);
            var hasData = true;
            while (skipping && hasData)
            {
                hasData = await itor.MoveNextAsync();
                skipping = predicate(itor.Current);
            }
            if (skipping) yield break;
            do
            {
                yield return itor.Current;
            } while (await itor.MoveNextAsync());
        }

        public static IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> seq, int n, CancellationToken cancelToken = default)
        {
            var counter = 0;
            return seq.TakeWhile(_ => counter++ < n, cancelToken);
        }

        public static async IAsyncEnumerable<T> TakeWhile<T>(this IAsyncEnumerable<T> seq, Func<T, bool> predicate,
            [EnumeratorCancellation] CancellationToken cancelToken = default)
        {
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            while(await itor.MoveNextAsync() && predicate(itor.Current))
                yield return itor.Current;
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancelToken = default) {
            var result = new List<T>(16);
            await source.Iter(result.Add, cancelToken);
            return result;
        }

        public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancelToken = default) {
            return (await source.ToListAsync(cancelToken)).ToArray();
        }

        public static async ValueTask<Option<T>> TryFind<T>(this IAsyncEnumerable<T> seq, Func<T,bool> finder, CancellationToken cancelToken = default)
        {
            await foreach(var i in seq.WithCancellation(cancelToken))
                if (finder(i))
                    return i;
            return None;
        }

        #region First

        public static async ValueTask<Option<T>> TryFirst<T>(this IAsyncEnumerable<T> source, CancellationToken cancelToken = default) {
            await using var enumerator = source.GetAsyncEnumerator(cancelToken);
            return await enumerator.MoveNextAsync() ? enumerator.Current : None;
        }

        public static async ValueTask<Option<T>> TryFirst<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate, CancellationToken cancelToken = default) {
            await using var enumerator = source.GetAsyncEnumerator(cancelToken);
            while(await enumerator.MoveNextAsync())
                if (predicate(enumerator.Current))
                    return enumerator.Current;
            return None;
        }

        public static async ValueTask<Option<T>> TryFirstAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> predicate, CancellationToken cancelToken = default) {
            await using var enumerator = source.GetAsyncEnumerator(cancelToken);
            while(await enumerator.MoveNextAsync())
                if (await predicate(enumerator.Current))
                    return enumerator.Current;
            return None;
        }

        #endregion

        public static ValueTask<Option<T>> TryFold<T>(this IAsyncEnumerable<T> seq, Func<T, T, T> folder, CancellationToken cancelToken = default) => 
            seq.TryFold(identity, folder, cancelToken);

        public static async ValueTask<Option<R>> TryFold<T, R>(this IAsyncEnumerable<T> seq, Func<T,R> seeder, Func<R, T, R> folder, CancellationToken cancelToken = default)
        {
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            if (!await itor.MoveNextAsync()) return None;
            var result = seeder(itor.Current);
            while (await itor.MoveNextAsync()) result = folder(result, itor.Current);
            return result;
        }

        public static async ValueTask<Option<T>> TryGetAt<T>(this IAsyncEnumerable<T> seq, int index, CancellationToken cancelToken = default)
        {
            if (index < 0) return None;

            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken))
                if (counter++ == index)
                    return i;
            return None;
        }

        public static async ValueTask<Option<T>> TryLast<T>(this IAsyncEnumerable<T> seq, CancellationToken cancelToken = default)
        {
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            Option<T> result = None;
            while (await itor.MoveNextAsync())
            {
                cancelToken.ThrowIfCancellationRequested();
                result = itor.Current;
            }
            return result;
        }

        public static ValueTask<Option<T>> TryMax<T>(this IAsyncEnumerable<T> seq, IComparer<T>? comparer = null, CancellationToken cancelToken = default) =>
            seq.TryMaxBy(identity, comparer, cancelToken);

        public static ValueTask<Option<T>> TryMaxBy<T, K>(this IAsyncEnumerable<T> seq, Func<T, K> getKey, IComparer<K>? comparer=null, CancellationToken cancelToken = default)
        {
            comparer ??= Comparer<K>.Default;
            return seq.TrySearch(getKey, (last,current) => comparer.Compare(current, last) > 0, cancelToken);
        }

        public static ValueTask<Option<T>> TryMin<T>(this IAsyncEnumerable<T> seq, IComparer<T>? comparer = null, CancellationToken cancelToken = default) => 
            seq.TryMinBy(identity, comparer, cancelToken);

        public static ValueTask<Option<T>> TryMinBy<T, K>(this IAsyncEnumerable<T> seq, Func<T, K> getKey, IComparer<K>? comparer=null, CancellationToken cancelToken = default)
        {
            comparer ??= Comparer<K>.Default;
            return seq.TrySearch(getKey, (last,current) => comparer.Compare(current, last) < 0, cancelToken);
        }

        public static ValueTask<Option<T>> TrySearch<T, K>(this IAsyncEnumerable<T> seq, Func<T, K> getKey, Func<K,K,bool> chooseCurrent, CancellationToken cancelToken = default)
        {
            return seq.TryFold(identity, selector, cancelToken);
            T selector(T last, T current) => chooseCurrent(getKey(last), getKey(current)) ? current : last;
        }

        public static ValueTask<Option<T>> TrySingle<T>(this IAsyncEnumerable<T> seq, CancellationToken cancelToken = default) =>
            seq.TrySingle(_ => true, cancelToken);

        public static async ValueTask<Option<T>> TrySingle<T>(this IAsyncEnumerable<T> seq, Func<T, bool> predicate, CancellationToken cancelToken = default)
        {
            await using var itor = seq.Where(predicate, cancelToken).GetAsyncEnumerator(cancelToken);
            if (!await itor.MoveNextAsync()) return None;
            var result = itor.Current;
            return await itor.MoveNextAsync() ? throw new InvalidOperationException("The sequence contains more than one element.") : result;
        }

        public static async ValueTask<Option<R>> TrySum<T, R>(this IAsyncEnumerable<T> seq, Func<T, R> getValue, CancellationToken cancelToken = default)
            where R: INumber<R> {
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            if (!await itor.MoveNextAsync()) return None;
            
            var sum = getValue(itor.Current);
            while(await itor.MoveNextAsync()) sum += getValue(itor.Current);
            return sum;
        }

        #region Where methods
        public static async IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken))
                if (predicate(i))
                    yield return i;
        }

        public static IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> source, Func<T, Task<bool>> predicate, CancellationToken cancelToken = default) => 
            source.WhereAsync(x => predicate(x).ToValue(), cancelToken);

        public static async IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask<bool>> predicate, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            await foreach (var i in source.WithCancellation(cancelToken))
                if (await predicate(i))
                    yield return i;
        }

        #endregion
    }
}