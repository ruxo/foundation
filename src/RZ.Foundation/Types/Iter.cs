using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RZ.Foundation.Types
{
    /// <summary>
    /// Iterable is basically IEnumerable with cacheability.
    /// </summary>
    public sealed class Iter<T> : IEnumerable<T>
    {
        IEnumerable<T> source;
        public Iter(IEnumerable<T> origin) => source = origin;

        #region IEnumerable<T> support

        public IEnumerator<T> GetEnumerator() => source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public Iter<T> EnableCache() => EnableCache(() => new ThreadUnsafeFetcher(source.GetEnumerator()));
        public Iter<T> EnableConcurrencyCache() => EnableCache(() => new ConcurrencyFetcher(source.GetEnumerator()));

        Iter<T> EnableCache(Func<IFetcher> fetcherFactory) {
            if (!(source is IteratorCache || source is T[] || source is IReadOnlyCollection<T>))
                source = new IteratorCache(fetcherFactory());
            return this;
        }

        sealed class IteratorCache : IEnumerable<T>
        {
            readonly IFetcher fetcher;

            public IteratorCache(IFetcher fetcher) {
                this.fetcher = fetcher;
            }

            public IEnumerator<T> GetEnumerator() => new FollowerIterator(fetcher);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        interface IFetcher : IDisposable
        {
            int CacheCount { get; }
            T GetAt(int index);
            bool MoveNext();
        }

        sealed class ThreadUnsafeFetcher : IFetcher {
            readonly IEnumerator<T> source;
            readonly List<T> cache = new List<T>();
            bool isAvailable = true;

            public ThreadUnsafeFetcher(IEnumerator<T> source) {
                this.source = source;
            }

            public void Dispose() { /* nothing to dispose */ }

            public int CacheCount => cache.Count;

            public T GetAt(int index) => cache[index];

            public bool MoveNext() {
                if (isAvailable) {
                    isAvailable = source.MoveNext();
                    if (isAvailable)
                        cache.Add(source.Current);
                    else
                        source.Dispose();
                }
                return isAvailable;
            }
        }

        sealed class ConcurrencyFetcher : IFetcher {
            readonly IEnumerator<T> source;
            readonly List<T> cache = new List<T>();
            bool isAvailable = true;
            readonly ReaderWriterLockSlim moveRequest = new ReaderWriterLockSlim();
            readonly ManualResetEventSlim updateSignal = new ManualResetEventSlim(false);

            public ConcurrencyFetcher(IEnumerator<T> source) {
                this.source = source;
            }

            public void Dispose() {
                moveRequest.Dispose();
            }

            public int CacheCount {
                get {
                    moveRequest.EnterReadLock();
                    try {
                        return cache.Count;
                    }
                    finally {
                        moveRequest.ExitReadLock();
                    }
                }
            }

            public T GetAt(int index) {
                    moveRequest.EnterReadLock();
                    try {
                        return cache[index];
                    }
                    finally {
                        moveRequest.ExitReadLock();
                    }
            }

            public bool MoveNext() {
                if (!isAvailable) return isAvailable;

                updateSignal.Reset();
                var firstRequest = moveRequest.TryEnterWriteLock(1);
                if (firstRequest) {
                    try {
                        isAvailable = source.MoveNext();
                        if (isAvailable)
                            cache.Add(source.Current);
                        else
                            source.Dispose();
                    }
                    finally {
                        moveRequest.ExitWriteLock();
                        updateSignal.Set();
                    }
                }
                else
                    updateSignal.Wait();
                return isAvailable;
            }
        }

        sealed class FollowerIterator : IEnumerator<T> {
            readonly IFetcher fetcher;
            int index = -1;

            public FollowerIterator(IFetcher fetcher) {
                this.fetcher = fetcher;
            }
            public bool MoveNext() {
                if (++index < fetcher.CacheCount || fetcher.MoveNext()) {
                    Current = fetcher.GetAt(index);
                    return true;
                }
                else
                    return false;
            }

            public void Reset() {
                index = -1;
            }

            public T Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose() { /*  */ }
        }
    }
}