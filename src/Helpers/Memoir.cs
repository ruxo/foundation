using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RZ.Foundation.Extensions;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Helpers
{
    public static class Memoir
    {
        public static Func<A, B> From<A, B>(Func<A, B> loader, Func<A, Option<B>> cache, Action<A, B> store) => x => cache(x).Get(Identity, () => SideEffect<B>(result => store(x, result))(loader(x)));

        public static Func<A, Task<B>> FromAsync<A, B>(Func<A, Task<B>> loader, Func<A, Task<Option<B>>> cache, Func<A, B, Task> store) => async x => {
            var v = await cache(x);
            if (v.IsSome)
                return v.Get();
            var result = await loader(x);
            await store(x, result);
            return result;
        };

        public static Func<A, B> DictWith<A, B>(Func<A, B> loader) {
            var cache = new Dictionary<A,B>();
            return From( loader
                       , y => cache.Get(y)
                       , (a, b) => cache[a] = b
                       );
        }
    }
}