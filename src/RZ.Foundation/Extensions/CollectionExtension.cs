using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        [Obsolete("use Iter instead")]
        public static void ForEach<T>(this IEnumerable<T> seq, Action<T> handler) => seq.Iter(handler);

        [Obsolete("use Iter instead")]
        public static void ForEachIndex<T>(this IEnumerable<T> seq, Action<T, int> handler) => seq.Iter((i, x) => handler(x, i));

        /// <summary>
        /// Syntactic sugar for "Select"
        /// </summary>
        public static IEnumerable<V> Map<T, V>(this IEnumerable<T> seq, Func<T, V> mapper) => seq.Select(mapper);

        /// <summary>
        /// Syntactic sugar for "SelectMany"
        /// </summary>
        public static IEnumerable<V> Chain<T, V>(this IEnumerable<T> seq, Func<T, IEnumerable<V>> binder) => seq.SelectMany(binder);

        /// <summary>
        /// Remove an element from an array. Note it is not atomic operation.
        /// </summary>
        /// <param name="array">an array</param>
        /// <param name="n">position to remove.</param>
        /// <typeparam name="T">Type parameter of array</typeparam>
        /// <returns>Return a new sequence without element at n.  If n is out of range, a new array of same content is returned.</returns>
        public static IEnumerable<T> RemoveAt<T>(this IEnumerable<T> array, int n) => array.Where((_, i) => i != n);

        public static Option<T> Get<TKey, T>(this IDictionary<TKey, T> dict, TKey key) => dict.TryGetValue(key);

        /// <summary>
        /// Split a sequence into two array by a predicate.
        /// </summary>
        /// <param name="e">a sequence</param>
        /// <param name="partitioner">a predicate for splitting the sequence</param>
        /// <typeparam name="T">type parameter for <paramref name="e"/></typeparam>
        /// <returns>A tuple of split arrays. First item is an array corresponding to true value of predicate. Second item is the rest.</returns>
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

        /// <summary>
        /// Try retrieving the first element from a sequence.
        /// </summary>
        /// <param name="seq">a sequence</param>
        /// <typeparam name="T">type parameter of seq</typeparam>
        /// <returns>an option value of first element.</returns>
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
        public static Option<T> TryFirst<T>(this IEnumerable<T> seq, Func<T, bool> predicate) => seq.Where(predicate).TryFirst();

        /// <summary>
        ///
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> data, int size) {
            using var itor = data.GetEnumerator();
            while (itor.MoveNext())
                yield return Take(itor, size - 1, itor.Current).ToArray();
        }

        static IEnumerable<T> Take<T>(IEnumerator<T> itor, int size, T init) {
            yield return init;
            for (var i = 0; i < size && itor.MoveNext(); ++i)
                yield return itor.Current;
        }
    }
}
