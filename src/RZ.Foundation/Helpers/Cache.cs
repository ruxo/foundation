using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RZ.Foundation.Helpers
{
    /// <summary>
    /// Cache module
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public static class Cache
    {
        public sealed class Context<T> : IDisposable
        {
            Func<T> getter;
            readonly IDisposable disposable;

            public Context(Func<T> getter, IDisposable disposable) {
                this.getter = getter;
                this.disposable = disposable;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Get() => getter();

            public void Dispose() {
                disposable.Dispose();
                getter = () => throw new ObjectDisposedException("data");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Context<T> Of<T>(Func<T> loader, TimeSpan lifetime, Func<DateTime>? timer = null) => Of(loader, _ => lifetime, timer);

        public static Context<T> Of<T>(Func<T> loader, Func<T, TimeSpan> getLifetime, Func<DateTime>? timer = null) {
            var locker = new ReaderWriterLockSlim();
            T data = default!;
            var expired = DateTime.MinValue;
            var now = timer ?? DefaultTimer;

            T getter() {
                try {
                    locker.EnterUpgradeableReadLock();
                    if (expired >= now()) return data;

                    try {
                        locker.EnterWriteLock();
                        (data as IDisposable)?.Dispose();
                        data = loader();
                        expired = now() + getLifetime(data);
                        return data;
                    }
                    finally {
                        locker.ExitWriteLock();
                    }
                }
                finally {
                    locker.ExitUpgradeableReadLock();
                }
            }

            return new(getter, locker);
        }

        public sealed class AsyncContext<T> : IDisposable
        {
            Func<Task<T>> getter;
            readonly IDisposable disposable;

            public AsyncContext(Func<Task<T>> getter, IDisposable disposable) {
                this.getter = getter;
                this.disposable = disposable;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Task<T> Get() => getter();

            public void Dispose() {
                disposable.Dispose();
                getter = () => throw new ObjectDisposedException("data");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AsyncContext<T> OfAsync<T>(Func<Task<T>> loader, TimeSpan lifetime, Func<DateTime>? timer = null) => OfAsync(loader, _ => lifetime, timer);
        
        public static AsyncContext<T> OfAsync<T>(Func<Task<T>> loader, Func<T, TimeSpan> getLifetime, Func<DateTime>? timer = null) {
            var locker = new ReaderWriterLockSlim();
            T data = default!;
            var expired = DateTime.MinValue;
            var now = timer ?? DefaultTimer;
            async Task<T> getter() {
                try {
                    locker.EnterUpgradeableReadLock();
                    if (expired >= now()) return data;

                    try {
                        locker.EnterWriteLock();
                        if (data is IAsyncDisposable disposeAsync)
                            await disposeAsync.DisposeAsync();
                        else
                            (data as IDisposable)?.Dispose();

                        data = await loader();
                        expired = now() + getLifetime(data);
                        return data;
                    }
                    finally {
                        locker.ExitWriteLock();
                    }
                }
                finally {
                    locker.ExitUpgradeableReadLock();
                }
            }
            return new(getter, locker);
        }
        
        static DateTime DefaultTimer() => DateTime.Now;
    }
}