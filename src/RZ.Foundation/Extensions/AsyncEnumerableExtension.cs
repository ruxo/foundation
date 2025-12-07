using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RZ.Foundation.Types;

// ReSharper disable VariableHidesOuterVariable
// ReSharper disable PossibleMultipleEnumeration

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RZ.Foundation.Extensions;

[PublicAPI]
public static class AsyncEnumerableExtension
{
    public static bool IsKnownEmpty<T>(this IAsyncEnumerable<T> source)
        => ReferenceEquals(source, AsyncEnumerable.Empty<T>());

    [Obsolete("Use ToAsyncEnumerable instead")]
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> source)
        => source.ToAsyncEnumerable();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> Average<T>(this IAsyncEnumerable<T> seq) where T : INumber<T>
        => seq.AverageBy((x, _) => x);

    extension<A>(IAsyncEnumerable<A> seq)
    {
        #region Aggregation

        public IAsyncEnumerable<B> Aggregate<B>(Func<B, A, B> folder, B seed) {
            return seq.IsKnownEmpty() ? AsyncEnumerable.Empty<B>() : Impl(seq, folder, seed);

            static async IAsyncEnumerable<B> Impl(IAsyncEnumerable<A> seq, Func<B, A, B> folder, B seed, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var accumulator = seed;
                await foreach (var item in seq.WithCancellation(cancelToken)){
                    accumulator = folder(accumulator, item);
                    yield return accumulator;
                }
            }
        }

        public IAsyncEnumerable<B> AggregateAsync<B>(Func<B, A, CancellationToken, ValueTask<B>> folder, B seed) {
            return seq.IsKnownEmpty() ? AsyncEnumerable.Empty<B>() : Impl(seq, folder, seed);

            static async IAsyncEnumerable<B> Impl(IAsyncEnumerable<A> seq, Func<B, A, CancellationToken, ValueTask<B>> folder, B seed, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var accumulator = seed;
                await foreach (var item in seq.WithCancellation(cancelToken)){
                    accumulator = await folder(accumulator, item, cancelToken).ConfigureAwait(false);
                    yield return accumulator;
                }
            }
        }

        #endregion

        public async ValueTask<bool> All(Func<A, int, bool> predicate, CancellationToken cancelToken = default)
            => (await seq.TryFind((i, n) => !predicate(i, n), cancelToken).ConfigureAwait(false)).IsNone;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IAsyncEnumerable<(A, A)> AllPairs(IAsyncEnumerable<A> b)
            => seq.Zip<A, A, (A, A)>(b, (lhs, rhs, _) => new((lhs, rhs)));

        public async ValueTask<bool> Any(Func<A, int, bool> predicate, CancellationToken cancelToken = default)
            => (await seq.TryFind(predicate, cancelToken).ConfigureAwait(false)).IsSome;

        public async ValueTask<bool> Any(CancellationToken cancelToken = default) {
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            return await itor.MoveNextAsync().ConfigureAwait(false);
        }

        public IAsyncEnumerable<A> AppendAsync(ValueTask<A> element) {
            return seq.IsKnownEmpty() ? seq : Impl(seq, element);

            static async IAsyncEnumerable<A> Impl(IAsyncEnumerable<A> seq, ValueTask<A> element, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                await foreach (var i in seq.WithCancellation(cancelToken))
                    yield return i;
                yield return await element.ConfigureAwait(false);
            }
        }

        #region Average methods

        public async ValueTask<TAverage> AverageBy<TAverage>(Func<A, int, TAverage> getter, CancellationToken cancelToken = default)
            where TAverage : INumber<TAverage> {
            var v = TAverage.Zero;
            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken)){
                v += getter(i, counter);
                counter++;
            }
            return v / TAverage.CreateChecked(counter);
        }

        public async ValueTask<Option<TAverage>> TryAverageBy<TAverage>(Func<A, int, TAverage> getter, CancellationToken cancelToken = default)
            where TAverage : INumber<TAverage> {
            var v = TAverage.Zero;
            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken)){
                v += getter(i, counter);
                counter++;
            }
            return counter == 0 ? None : Some(v / TAverage.CreateChecked(counter));
        }

        #endregion
    }

    #region Choose methods

    extension<A>(IEnumerable<A> seq)
    {
        public IAsyncEnumerable<B> ChooseAsync<B>(Func<A, int, CancellationToken, ValueTask<Option<B>>> selector) {
            return Impl(seq, selector);

            static async IAsyncEnumerable<B> Impl(IEnumerable<A> source, Func<A, int, CancellationToken, ValueTask<Option<B>>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var counter = 0;
                foreach (var i in source){
                    cancelToken.ThrowIfCancellationRequested();

                    var result = await selector(i, counter++, cancelToken).ConfigureAwait(false);
                    if (result.IsSome)
                        yield return result.Get();
                }
            }
        }
    }

    extension<A>(IAsyncEnumerable<A> seq)
    {
        public IAsyncEnumerable<B> ChooseAsync<B>(Func<A, int, CancellationToken, ValueTask<Option<B>>> selector) {
            return seq.IsKnownEmpty() ? AsyncEnumerable.Empty<B>() : Impl(seq, selector);

            static async IAsyncEnumerable<B> Impl(IAsyncEnumerable<A> seq, Func<A, int, CancellationToken, ValueTask<Option<B>>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var counter = 0;
                await foreach (var i in seq.WithCancellation(cancelToken)){
                    var result = await selector(i, counter++, cancelToken).ConfigureAwait(false);
                    if (result.IsSome)
                        yield return result.Get();
                }
            }
        }

        public IAsyncEnumerable<B> Choose<B>(Func<A, int, Option<B>> selector) {
            return seq.IsKnownEmpty() ? AsyncEnumerable.Empty<B>() : Impl(seq, selector);

            static async IAsyncEnumerable<B> Impl(IAsyncEnumerable<A> seq, Func<A, int, Option<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var counter = 0;
                await foreach (var i in seq.WithCancellation(cancelToken)){
                    var result = selector(i, counter++);
                    if (result.IsSome)
                        yield return result.Get();
                }
            }
        }
    }

    #endregion

    extension<A>(IAsyncEnumerable<A> seq)
    {
        public IAsyncEnumerable<A> Concat(IEnumerable<A> second) {
            return seq.IsKnownEmpty() ? seq : Impl(seq, second);

            static async IAsyncEnumerable<A> Impl(IAsyncEnumerable<A> seq, IEnumerable<A> second, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                await foreach (var i in seq.WithCancellation(cancelToken))
                    yield return i;
                foreach (var i in second)
                    yield return i;
            }
        }
    }

    [Obsolete("Use ContainsAsync instead"), MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<bool> Contains<T>(this IAsyncEnumerable<T> seq, T value, CancellationToken cancelToken = default)
        => seq.ContainsAsync(value, cancellationToken: cancelToken);

    extension<T>(IAsyncEnumerable<T> seq)
    {
        /// Same as calling <seealso cref="AsyncEnumerable.CountAsync{T}(IAsyncEnumerable{T}, CancellationToken)" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> Count(CancellationToken cancelToken = default)
            => seq.CountAsync(cancelToken);

        /// <summary>
        ///  Same as calling <seealso cref="AsyncEnumerable.CountAsync{T}(IAsyncEnumerable{T}, Func{T, bool}, CancellationToken)"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<int> Count(Func<T, bool> predicate, CancellationToken cancelToken = default)
            => seq.CountAsync(predicate, cancelToken);
    }

    #region Flatten methods

    extension<A>(IEnumerable<A> seq)
    {
        public IAsyncEnumerable<B> FlattenT<B>(Func<A, int, CancellationToken, ValueTask<IEnumerable<B>>> selector) {
            return Impl(seq, selector);

            static async IAsyncEnumerable<B> Impl(IEnumerable<A> seq, Func<A, int, CancellationToken, ValueTask<IEnumerable<B>>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var counter = 0;
                foreach (var i in seq){
                    cancelToken.ThrowIfCancellationRequested();
                    foreach (var inner in await selector(i, counter++, cancelToken).ConfigureAwait(false))
                        yield return inner;
                }
            }
        }

        public IAsyncEnumerable<B> FlattenT<B>(Func<A, int, IAsyncEnumerable<B>> selector) {
            return Impl(seq, selector);

            static async IAsyncEnumerable<B> Impl(IEnumerable<A> seq, Func<A, int, IAsyncEnumerable<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var counter = 0;
                foreach (var i in seq){
                    cancelToken.ThrowIfCancellationRequested();
                    await foreach (var inner in selector(i, counter++).WithCancellation(cancelToken))
                        yield return inner;
                }
            }
        }
    }

    extension<A>(IAsyncEnumerable<A> seq)
    {
        public IAsyncEnumerable<B> FlattenAsync<B>(Func<A, int, CancellationToken, ValueTask<IEnumerable<B>>> selector) {
            return seq.IsKnownEmpty() ? AsyncEnumerable.Empty<B>() : Impl(seq, selector);

            static async IAsyncEnumerable<B> Impl(IAsyncEnumerable<A> seq, Func<A, int, CancellationToken, ValueTask<IEnumerable<B>>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var counter = 0;
                await foreach (var i in seq.WithCancellation(cancelToken))
                foreach (var inner in await selector(i, counter++, cancelToken).ConfigureAwait(false))
                    yield return inner;
            }
        }

        public IAsyncEnumerable<B> Flatten<B>(Func<A, int, IEnumerable<B>> selector) {
            return seq.IsKnownEmpty() ? AsyncEnumerable.Empty<B>() : Impl(seq, selector);

            static async IAsyncEnumerable<B> Impl(IAsyncEnumerable<A> seq, Func<A, int, IEnumerable<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var counter = 0;
                await foreach (var i in seq.WithCancellation(cancelToken))
                foreach (var inner in selector(i, counter++))
                    yield return inner;
            }
        }

        public IAsyncEnumerable<B> Flatten<B>(Func<A, int, IAsyncEnumerable<B>> selector) {
            return seq.IsKnownEmpty() ? AsyncEnumerable.Empty<B>() : Impl(seq, selector);

            static async IAsyncEnumerable<B> Impl(IAsyncEnumerable<A> seq, Func<A, int, IAsyncEnumerable<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
                var counter = 0;
                await foreach (var i in seq.WithCancellation(cancelToken))
                await foreach (var inner in selector(i, counter++).WithCancellation(cancelToken))
                    yield return inner;
            }
        }
    }

    #endregion

    public static IAsyncEnumerable<B> MapAsync<A, B>(this IEnumerable<A> seq, Func<A, int, CancellationToken, ValueTask<B>> selector) {
        return Impl(seq, selector);

        static async IAsyncEnumerable<B> Impl(IEnumerable<A> seq, Func<A, int, CancellationToken, ValueTask<B>> selector, [EnumeratorCancellation] CancellationToken cancelToken = default) {
            var counter = 0;
            foreach (var i in seq){
                cancelToken.ThrowIfCancellationRequested();
                var result = await selector(i, counter++, cancelToken).ConfigureAwait(false);
                yield return result;
            }
        }
    }

    #region Iter methods

    extension<T>(IAsyncEnumerable<T> seq)
    {
        public async ValueTask Iter(Action<T, int> action, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return;

            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken)) action(i, counter++);
        }

        public async ValueTask Iter(Func<T, int, CancellationToken, ValueTask> action, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return;

            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken)) await action(i, counter++, cancelToken).ConfigureAwait(false);
        }
    }

    #endregion

    extension<T>(IAsyncEnumerable<T> seq)
    {
        public async ValueTask<Option<T>> TryFind(Func<T, int, bool> finder, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return None;

            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken))
                if (finder(i, counter++))
                    return i;
            return None;
        }

        #region First

        public async ValueTask<Option<T>> TryFirst(CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return None;

            await using var enumerator = seq.GetAsyncEnumerator(cancelToken);
            return await enumerator.MoveNextAsync().ConfigureAwait(false) ? enumerator.Current : None;
        }

        public async ValueTask<Option<T>> TryFirst(Func<T, bool> predicate, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return None;

            await using var enumerator = seq.GetAsyncEnumerator(cancelToken);
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                if (predicate(enumerator.Current))
                    return enumerator.Current;
            return None;
        }

        public async ValueTask<Option<T>> TryFirstAsync(Func<T, CancellationToken, ValueTask<bool>> predicate, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return None;

            await using var enumerator = seq.GetAsyncEnumerator(cancelToken);
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
                if (await predicate(enumerator.Current, cancelToken).ConfigureAwait(false))
                    return enumerator.Current;
            return None;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Option<T>> TryFold(Func<T, T, int, T> folder, CancellationToken cancelToken = default)
            => seq.TryFold(identity, folder, cancelToken);

        public async ValueTask<Option<R>> TryFold<R>(Func<T, R> seeder, Func<R, T, int, R> folder, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return None;

            var counter = 0;
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            if (!await itor.MoveNextAsync().ConfigureAwait(false)) return None;
            var result = seeder(itor.Current);
            while (await itor.MoveNextAsync().ConfigureAwait(false)) result = folder(result, itor.Current, ++counter);
            return result;
        }

        public async ValueTask<Option<T>> TryGetAt(int index, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty() || index < 0) return None;

            var counter = 0;
            await foreach (var i in seq.WithCancellation(cancelToken))
                if (counter++ == index)
                    return i;
            return None;
        }

        public async ValueTask<Option<T>> TryLast(CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return None;

            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            Option<T> result = None;
            while (await itor.MoveNextAsync().ConfigureAwait(false)){
                cancelToken.ThrowIfCancellationRequested();
                result = itor.Current;
            }
            return result;
        }

        public ValueTask<Option<T>> TryMax(IComparer<T>? comparer = null, CancellationToken cancelToken = default)
            => seq.TryMaxBy(identity, comparer, cancelToken);

        public ValueTask<Option<T>> TryMaxBy<K>(Func<T, K> getKey, IComparer<K>? comparer = null, CancellationToken cancelToken = default) {
            comparer ??= Comparer<K>.Default;
            return seq.TrySearch(getKey, (last, current, _) => comparer.Compare(current, last) > 0, cancelToken);
        }

        public ValueTask<Option<T>> TryMin(IComparer<T>? comparer = null, CancellationToken cancelToken = default) =>
            seq.TryMinBy(identity, comparer, cancelToken);

        public ValueTask<Option<T>> TryMinBy<K>(Func<T, K> getKey, IComparer<K>? comparer = null, CancellationToken cancelToken = default) {
            comparer ??= Comparer<K>.Default;
            return seq.TrySearch(getKey, (last, current, _) => comparer.Compare(current, last) < 0, cancelToken);
        }

        public ValueTask<Option<T>> TrySearch<K>(Func<T, K> getKey, Func<K, K, int, bool> chooseCurrent, CancellationToken cancelToken = default) {
            return seq.TryFold(identity, Selector, cancelToken);
            T Selector(T last, T current, int n) => chooseCurrent(getKey(last), getKey(current), n) ? current : last;
        }

        public ValueTask<Outcome<T>> TrySingle(CancellationToken cancelToken = default)
            => seq.TrySingle((_, _) => true, cancelToken);

        public async ValueTask<Outcome<T>> TrySingle(Func<T, int, bool> predicate, CancellationToken cancelToken = default) {
            if (seq.IsKnownEmpty()) return new ErrorInfo(StandardErrorCodes.NotFound);

            await using var itor = seq.Where(predicate).GetAsyncEnumerator(cancelToken);
            if (!await itor.MoveNextAsync().ConfigureAwait(false)) return new ErrorInfo(StandardErrorCodes.NotFound);
            var result = itor.Current;
            return await itor.MoveNextAsync().ConfigureAwait(false) ? new ErrorInfo(StandardErrorCodes.ValidationFailed, "Multiple elements found") : result;
        }

        public async ValueTask<Option<R>> TrySum<R>(Func<T, int, R> getValue, CancellationToken cancelToken = default) where R : INumber<R> {
            if (seq.IsKnownEmpty()) return None;

            var counter = 0;
            await using var itor = seq.GetAsyncEnumerator(cancelToken);
            if (!await itor.MoveNextAsync().ConfigureAwait(false)) return None;

            var sum = getValue(itor.Current, counter++);
            while (await itor.MoveNextAsync().ConfigureAwait(false)) sum += getValue(itor.Current, counter++);
            return sum;
        }
    }
}