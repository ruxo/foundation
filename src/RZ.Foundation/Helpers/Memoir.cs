using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RZ.Foundation.Extensions;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Helpers
{
    public static class Memoir
    {
        public interface ICache<in A,B>
        {
            Option<B> Get(A x);
            void Store(A x, B result);
        }

        public interface ICacheAsync<A, B>
        {
            Task<Option<B>> GetAsync(A x);
            Task StoreAsync(A x, B result);
        }

        public interface ILocker<A>
        {
            bool TryEnter(A x);
            void Wait(A x);
            void Leave(A x);
        }
        public sealed class DictionaryCache<A, B> : ICache<A, B>
        {
            readonly Dictionary<A,B> cache = new Dictionary<A, B>();
            public Option<B> Get(A x) => cache.Get(x);
            public void Store(A x, B result) => cache[x] = result;
        }
        public sealed class MultithreadLocker<A> : ILocker<A> {
            sealed class Locker
            {
                public int Lock;
            }
            readonly ConcurrentDictionary<A,Locker> lockers = new ConcurrentDictionary<A, Locker>();

            public bool TryEnter(A x) {
                var @lock = lockers.GetOrAdd(x, _ => new Locker());
                return Interlocked.Increment(ref @lock.Lock) == 1;
            }

            public void Wait(A x) {
                var @lock = lockers[x];
                while (@lock.Lock != 0) {
#if NETSTANDARD2_0
                    Thread.Yield();
#else
                    // Lower .NET 2.2 does not support thread :( so just spin CPU...
#endif
                }
            }

            public void Leave(A x) => Interlocked.Exchange(ref lockers[x].Lock, 0);
        }

        static B ExecuteMemoir<A,B>(Func<A,B> loader, ICache<A,B> cache, A x) =>
            cache.Get(x).Get(Identity, () => SideEffect<B>(result => cache.Store(x, result))(loader(x)));

        public static Func<A, B> From<A, B>(Func<A, B> loader, ICache<A, B> cache) => x => ExecuteMemoir(loader, cache, x);

        public static Func<A, B> From<A, B>(Func<A, B> loader, ICache<A,B> cache, ILocker<A> locker) => x => {
retry:
            var v = cache.Get(x);
            if (v.IsSome)
                return v.Get();
            if (locker.TryEnter(x))
                try {
                    return ExecuteMemoir(loader, cache, x);
                }
                finally {
                    locker.Leave(x);
                }
            else {
                locker.Wait(x);
                goto retry;
            }
        };

        public static async Task<B> ExecuteMemoirAsync<A, B>(Func<A, Task<B>> loader, ICacheAsync<A, B> cache, A x) {
            var v = await cache.GetAsync(x);
            if (v.IsSome)
                return v.Get();
            var result = await loader(x);
            await cache.StoreAsync(x, result);
            return result;
        }

        public static Func<A, Task<B>> FromAsync<A, B>(Func<A, Task<B>> loader, ICacheAsync<A,B> cache) => x => ExecuteMemoirAsync(loader, cache, x);

        public static Func<A, Task<B>> FromAsync<A, B>(Func<A, Task<B>> loader, ICacheAsync<A,B> cache, ILocker<A> locker) => async x => {
retry:
            var v = await cache.GetAsync(x);
            if (v.IsSome)
                return v.Get();
            if (locker.TryEnter(x))
                try {
                    return await ExecuteMemoirAsync(loader, cache, x);
                }
                finally {
                    locker.Leave(x);
                }
            else {
                locker.Wait(x);
                goto retry;
            }
        };

        public static Func<A, B> DictWith<A, B>(Func<A, B> loader) => From(loader, new DictionaryCache<A,B>());
    }
}