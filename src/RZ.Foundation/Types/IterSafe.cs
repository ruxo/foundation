using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Types
{
    /// <summary>
    /// Cached iterable for concurrency scenario
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class IterSafe<T> : IEnumerable<T>, IDisposable
    {
        readonly IEnumerable<T> source;

        public IterSafe(IEnumerable<T> origin) {
            source = origin is IterSafe<T> || origin is T[] || origin is IReadOnlyCollection<T>
                         ? origin
                         : new IteratorCache(origin.GetEnumerator());
        }

        // support common data conversion
        public static implicit operator IterSafe<T>(T[] data) => new IterSafe<T>(data);
        public static implicit operator IterSafe<T>(List<T> data) => new IterSafe<T>(data);
        public static implicit operator IterSafe<T>(HashSet<T> data) => new IterSafe<T>(data);

        public IEnumerator<T> GetEnumerator() => source.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose() {
            if (source is IDisposable disposable) disposable.Dispose();
        }

        sealed class IteratorCache : IEnumerable<T>, IDisposable
        {
            readonly IEnumerator<T> source;
            readonly List<T> cache = new List<T>(16 /* my guess.. median usage size? */);
            Option<T> fetched = None<T>();
            readonly ReaderWriterLockSlim moveRequest = new ReaderWriterLockSlim();

            public IteratorCache(IEnumerator<T> source) {
                this.source = source;
            }

            public IEnumerator<T> GetEnumerator() {
                var readIndex = 0;
                while(readIndex < cache.Count || MoveNext())
                    yield return cache[readIndex++];
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Dispose() {
                moveRequest.Dispose();
            }

            bool MoveNext() {
                if (moveRequest.TryEnterWriteLock(1))
                    try {
                        fetched = source.MoveNext() ? source.Current : None<T>();
                        fetched.Then(cache.Add);
                        return fetched.IsSome;
                    }
                    finally {
                        moveRequest.ExitWriteLock();
                    }
                else {
                    moveRequest.EnterReadLock();
                    try {
                        return fetched.IsSome;
                    }
                    finally {
                        moveRequest.ExitReadLock();
                    }
                }
            }
        }
    }
}