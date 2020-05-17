using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Extensions {
    public static class CollectionExtension{
        public static void Clear<T>(this ConcurrentQueue<T> queue){
            while(queue.TryDequeue(out _)) { }
        }
        public static void ForEach<T>(this IEnumerable<T> seq, Action<T> handler){
            foreach (var item in seq)
                handler(item);
        }
        public static void ForEachIndex<T>(this IEnumerable<T> seq, Action<T,int> handler){
            var index = 0;
            foreach (var item in seq)
                handler(item, index++);
        }

        public static IEnumerable<V> Map<T, V>(this IEnumerable<T> seq, Func<T, V> mapper) => seq.Select(mapper);
        public static IEnumerable<V> Chain<T, V>(this IEnumerable<T> seq, Func<T, IEnumerable<V>> binder) => seq.SelectMany(binder);

        public static T[] RemoveAt<T>(this IEnumerable<T> array, int n) => array.Take(n).Skip(n + 1).ToArray();

        public static Option<T> Get<TKey, T>(this IDictionary<TKey, T> dict, TKey key) => dict.TryGetValue(key, out var result) ? Some(result) : None<T>();

        public static Option<T> Find<T>(this IList<T> collection, Func<T, bool> predicate) {
            foreach(var i in collection)
                if (predicate(i))
                    return Some(i);
            return None<T>();
        }

        public static IEnumerable<B> Choose<A, B>(this IEnumerable<A> array, Func<A, Option<B>> chooser) =>
            from i in array
            let opt = chooser(i)
            where opt.IsSome
            select opt.Get();

        public static IEnumerable<B> Choose<A, B>(this IEnumerable<A> array, Func<A, int, Option<B>> chooser) {
            var count = 0;
            foreach (var i in array) {
                var opt = chooser(i, count++);
                if (opt.IsSome)
                    yield return opt.Get();
            }
        }

        public static (T[], T[]) Partition<T>(this IEnumerable<T> e, Func<T, bool> partitioner) => e.Partition(partitioner, Identity, Identity);

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

        public static Option<T> TryFirst<T>(this IEnumerable<T> seq) {
            foreach (var item in seq) return Some(item);
            return Option<T>.None();
        }

        public static Option<T> TryFirst<T>(this IEnumerable<T> seq, Func<T, bool> predicate) => seq.Where(predicate).TryFirst();

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
