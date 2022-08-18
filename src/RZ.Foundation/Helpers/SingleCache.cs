using System;
using System.Threading;

// ignore type T
#pragma warning disable CS8618

namespace RZ.Foundation.Helpers {
    // ReSharper disable once UnusedType.Global
    /// <summary>
    /// Cache which reloads periodically by <c>lifetime</c>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Use Cache module instead")]
    public sealed class SingleCache<T>
    {
        readonly TimeSpan lifetime;
        readonly Func<T> loader;
        readonly ReaderWriterLockSlim locker = new();
        T data;
        DateTime expired = DateTime.MinValue;

        public SingleCache(TimeSpan lifetime, Func<T> loader)
        {
            this.lifetime = lifetime;
            this.loader = loader;
        }

        public T Value
        {
            get
            {
                try {
                    locker.EnterUpgradeableReadLock();
                    if (expired >= DateTime.Now) return data;

                    try {
                        locker.EnterWriteLock();
                        data = loader();
                        expired = DateTime.Now + lifetime;
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
        }
    }
}