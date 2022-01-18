using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LanguageExt;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Extensions {
    public static class CollectionExtension{
        /// <summary>
        /// Clear concurrent queue content
        /// </summary>
        /// <param name="queue"></param>
        /// <typeparam name="T"></typeparam>
        public static void Clear<T>(this ConcurrentQueue<T> queue){
            while(queue.TryDequeue(out _)) { }
        }

        /// <summary>
        /// Syntactic sugar for "Select"
        /// </summary>
        [Pure]
        public static IEnumerable<V> Map<T, V>(this IEnumerable<T> seq, Func<T, V> mapper) => seq.Select(mapper);

        /// <summary>
        /// Syntactic sugar for "SelectMany"
        /// </summary>
        [Pure]
        public static IEnumerable<V> Chain<T, V>(this IEnumerable<T> seq, Func<T, IEnumerable<V>> binder) => seq.SelectMany(binder);

        /// <summary>
        /// Remove an element from an array. Note it is not atomic operation.
        /// </summary>
        /// <param name="array">an array</param>
        /// <param name="n">position to remove.</param>
        /// <typeparam name="T">Type parameter of array</typeparam>
        /// <returns>Return a new sequence without element at n.  If n is out of range, a new array of same content is returned.</returns>
        [Pure]
        public static IEnumerable<T> RemoveAt<T>(this IEnumerable<T> array, int n) => array.Where((_, i) => i != n);

        [Pure]
        public static Option<T> Get<TKey, T>(this IDictionary<TKey, T> dict, TKey key) => dict.TryGetValue(key);
        [Pure]
        public static Option<T> Get<TKey, T>(this ImmutableDictionary<TKey, T> dict, TKey key) where TKey : notnull =>
            ((IImmutableDictionary<TKey, T>) dict).TryGetValue(key);
        [Pure]
        public static Option<T> Get<TKey, T>(this IImmutableDictionary<TKey, T> dict, TKey key) => dict.TryGetValue(key);

        /// <summary>
        /// Split a sequence into two array by a predicate.
        /// </summary>
        /// <param name="e">a sequence</param>
        /// <param name="partitioner">a predicate for splitting the sequence</param>
        /// <typeparam name="T">type parameter for <paramref name="e"/></typeparam>
        /// <returns>A tuple of split arrays. First item is an array corresponding to true value of predicate. Second item is the rest.</returns>
        [Pure]
        public static (T[], T[]) Partition<T>(this IEnumerable<T> e, Func<T, bool> partitioner) => e.Partition(partitioner, identity, identity);

        /// <summary>
        /// Split a sequence into two array by a predicate, and each element is transformed by either <paramref name="trueTransform"/>
        /// or <paramref name="falseTransform"/>
        /// </summary>
        /// <param name="e">a sequence</param>
        /// <param name="predicate">a predicate for splitting the sequence</param>
        /// <param name="trueTransform">A transformer for values passed predicate condition</param>
        /// <param name="falseTransform">A transformer for values rejected by predicate</param>
        /// <typeparam name="T">type parameter for <paramref name="e"/></typeparam>
        /// <typeparam name="A">Transformed type for <paramref name="trueTransform"/></typeparam>
        /// <typeparam name="B">Transformed type for <paramref name="falseTransform"/></typeparam>
        /// <returns>A tuple of split arrays. First item is an array corresponding to true value of predicate. Second item is the rest.</returns>
        [Pure]
        public static (A[], B[]) Partition<T, A, B>(this IEnumerable<T> e, Func<T, bool> predicate, Func<T, A> trueTransform, Func<T, B> falseTransform) {
            var trueResult = new List<A>();
            var falseResult = new List<B>();
            foreach(var i in e)
                if (predicate(i))
                    trueResult.Add(trueTransform(i));
                else
                    falseResult.Add(falseTransform(i));
            return (trueResult.ToArray(), falseResult.ToArray());
        }

        public static ImmutableDictionary<K, V> ToImmutableDictionary<K, V>(this IEnumerable<(K Key, V Value)> pairs) where K: notnull =>
            pairs.ToImmutableDictionary(k => k.Key, v => v.Value);

        /// <summary>
        /// Try retrieving the first element from a sequence.
        /// </summary>
        /// <param name="seq">a sequence</param>
        /// <typeparam name="T">type parameter of seq</typeparam>
        /// <returns>an option value of first element.</returns>
        [Pure]
        public static Option<T> TryFirst<T>(this IEnumerable<T> seq) {
            foreach (var item in seq) return Some(item);
            return None;
        }

        /// <summary>
        /// Try retrieving the first element that satisfies predicate from a sequence.
        /// </summary>
        /// <param name="seq">a sequence</param>
        /// <param name="predicate">condition for finding the first</param>
        /// <typeparam name="T">type parameter of seq</typeparam>
        /// <returns>an option value of first element.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> TryFirst<T>(this IEnumerable<T> seq, Func<T, bool> predicate) => seq.Where(predicate).TryFirst();

        /// <summary>
        /// Try retrieving the only single element from a sequence, assume that the sequence only has one element!
        /// </summary>
        /// <param name="seq">a sequence</param>
        /// <typeparam name="T">type parameter of seq</typeparam>
        /// <returns>an option value of the only element.</returns>
        [Pure]
        public static Option<T> TrySingle<T>(this IEnumerable<T> seq) {
            using var itor = seq.GetEnumerator();
            if (!itor.MoveNext()) return None;
            var result = itor.Current;
            return itor.MoveNext() ? throw new IndexOutOfRangeException("Sequence is expected to be a single element but it has many!") : result;
        }

        /// <summary>
        /// Try retrieving the only single element from a sequence, assume that the sequence only has one element!
        /// </summary>
        /// <param name="seq">a sequence</param>
        /// <param name="predicate">condition for finding the element</param>
        /// <typeparam name="T">type parameter of seq</typeparam>
        /// <returns>an option value of the only element.</returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Option<T> TrySingle<T>(this IEnumerable<T> seq, Func<T, bool> predicate) => seq.Where(predicate).TrySingle();

        /// <summary>
        /// Find an index number of the first element that satisfies the predicate.
        /// </summary>
        /// <param name="seq">A sequence to search</param>
        /// <param name="predicate">Condition predicate</param>
        /// <typeparam name="T">type parameter of seq</typeparam>
        /// <returns>an option value of first element index</returns>
        [Pure]
        public static Option<int> TryFindIndex<T>(this IEnumerable<T> seq, Predicate<T> predicate) {
            var index = -1;
            foreach (var i in seq) {
                ++index;
                if (predicate(i))
                    return index;
            }
            return None;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Pure]
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> data, int size) {
            using var itor = data.GetEnumerator();
            while (itor.MoveNext())
                yield return Take(itor, size - 1, itor.Current).ToArray();
        }

        [Pure]
        static IEnumerable<T> Take<T>(IEnumerator<T> itor, int size, T init) {
            yield return init;
            for (var i = 0; i < size && itor.MoveNext(); ++i)
                yield return itor.Current;
        }

        public static int GetCombinationHashCode<T>(this IEnumerable<T> seq) => seq.Aggregate(0, (hash, v) => hash ^ GetHashCode(v));
        public static int GetCollectionHashCode<T>(this IEnumerable<T> seq) => seq.Aggregate(0, (hash, v) => HashAndShift(hash, GetHashCode(v)));
        static int HashAndShift(int current, int newHash) => ((current ^ newHash) << 7) | ((current ^ newHash) >> (32 - 7));
        static int GetHashCode<T>(T? v) => v?.GetHashCode() ?? 0;
    }
}