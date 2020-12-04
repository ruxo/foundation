using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using RZ.Foundation.Extensions;
using static RZ.Foundation.Prelude;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Helpers
{
    public static class Memoir
    {
        public interface ICache<in A,B>
        {
            Option<B> Get(A x);
            void Store(A x, B result);
        }

        public interface ICacheAsync<in A, B>
        {
            Task<Option<B>> GetAsync(A x);
            Task StoreAsync(A x, B result);
        }

        public interface ILocker<in A> where A : notnull
        {
            bool TryEnter(A x);
            void Wait(A x);
            void Leave(A x);
        }
        public sealed class DictionaryCache<A, B> : ICache<A, B> where A : notnull
        {
            readonly Dictionary<A,B> cache = new();
            public Option<B> Get(A x) => cache.Get(x);
            public void Store(A x, B result) => cache[x] = result;
        }
        public sealed class MultithreadLocker<A> : ILocker<A> where A: notnull {
            sealed class Locker
            {
                public int Lock;
            }
            readonly ConcurrentDictionary<A,Locker> lockers = new();

            public bool TryEnter(A x) {
                var @lock = lockers.GetOrAdd(x, _ => new Locker());
                return Interlocked.Increment(ref @lock.Lock) == 1;
            }

            public void Wait(A x) {
                var @lock = lockers[x];
                while (@lock.Lock != 0) Thread.Yield();
            }

            public void Leave(A x) => Interlocked.Exchange(ref lockers[x].Lock, 0);
        }

        static B ExecuteMemoir<A,B>(Func<A,B> loader, ICache<A,B> cache, A x) =>
            cache.Get(x).Match(identity, () => SideEffect<B>(result => cache.Store(x, result))(loader(x)));

        public static Func<A, B> From<A, B>(Func<A, B> loader, ICache<A, B> cache) where A: notnull => x => ExecuteMemoir(loader, cache, x);

        public static Func<A, B> From<A, B>(Func<A, B> loader, ICache<A,B> cache, ILocker<A> locker) where A: notnull => x => {
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

        public static Func<A, Task<B>> FromAsync<A, B>(Func<A, Task<B>> loader, ICacheAsync<A,B> cache) where A: notnull => x => ExecuteMemoirAsync(loader, cache, x);

        public static Func<A, Task<B>> FromAsync<A, B>(Func<A, Task<B>> loader, ICacheAsync<A,B> cache, ILocker<A> locker) where A: notnull => async x => {
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

        public static Func<A, B> DictWith<A, B>(Func<A, B> loader) where A: notnull => From(loader, new DictionaryCache<A,B>());
    }
}