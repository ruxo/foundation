using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace RZ.Foundation.Extensions {
    public static class CollectionExtension{
        public static void Clear<T>(this ConcurrentQueue<T> queue){
            Contract.Requires(queue != null);

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
        public static T[] RemoveAt<T>(this T[] array, int n){
            return array.Take(n).Skip(n + 1).ToArray();
        }
        public static Option<T> Get<TKey, T>(this Dictionary<TKey, T> dict, TKey key){
            T result;
            return dict.TryGetValue(key, out result) ? (Option<T>) Option<T>.Some(result) : Option<T>.None();
        }
        public static Option<T> TryFirst<T>(this IEnumerable<T> seq, Func<T, bool> predicate){
            foreach (var item in seq.Where(predicate))
                return Option<T>.Some(item);
            return Option<T>.None();
        }
    }
}
