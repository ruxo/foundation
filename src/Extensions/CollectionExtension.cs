using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RZ.Foundation.Extensions {
    public static class CollectionExtension{
        public static void Clear<T>(this ConcurrentQueue<T> queue){
            T item;
            while(queue.TryDequeue(out item)) { }
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
        public static T[] RemoveAt<T>(this IEnumerable<T> array, int n) => array.Take(n).Skip(n + 1).ToArray();
        public static Option<T> Get<TKey, T>(this Dictionary<TKey, T> dict, TKey key) => dict.TryGetValue(key, out var result) ? result : Option<T>.None();

        public static Option<T> TryFirst<T>(this IEnumerable<T> seq, Func<T, bool> predicate){
            foreach (var item in seq.Where(predicate))
                return item;
            return Option<T>.None();
        }
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> data, int size) {
            using(var itor = data.GetEnumerator())
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
