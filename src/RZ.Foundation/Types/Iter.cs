using System.Collections;
using System.Collections.Generic;

namespace RZ.Foundation.Types
{
    /// <summary>
    /// Iterable is basically IEnumerable with cacheability.
    /// </summary>
    public sealed class Iter<T> : IEnumerable<T>
    {
        readonly IEnumerable<T> source;

        public Iter(IEnumerable<T> origin) {
            source = origin is IteratorCache || origin is T[] || origin is IReadOnlyCollection<T>
                         ? origin
                         : new IteratorCache(origin.GetEnumerator());
        }

        #region IEnumerable<T> support

        public IEnumerator<T> GetEnumerator() => source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        sealed class IteratorCache : IEnumerable<T>
        {
            readonly IEnumerator<T> source;
            readonly List<T> cache = new List<T>();

            public IteratorCache(IEnumerator<T> source) {
                this.source = source;
            }

            public IEnumerator<T> GetEnumerator() {
                var readIndex = 0;
                while (readIndex < cache.Count || MoveNext())
                    yield return cache[readIndex++];
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            bool MoveNext() {
                if (source.MoveNext()) {
                    cache.Add(source.Current);
                    return true;
                }
                else
                    return false;
            }
        }
    }
}