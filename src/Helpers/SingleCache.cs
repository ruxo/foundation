using System;

namespace RZ.Foundation.Helpers {
    public sealed class SingleCache<T>
    {
        readonly TimeSpan _lifetime;
        readonly Func<T> _loader;
        readonly object _locker = new object();
        T data;
        DateTime expired = DateTime.MinValue;

        public SingleCache(TimeSpan lifetime, Func<T> loader)
        {
            _lifetime = lifetime;
            _loader = loader;
        }

        public T Value
        {
            get
            {
                if (expired > DateTime.Now) return data;

                lock (_locker)
                    if (expired < DateTime.Now)
                    {
                        data = _loader();
                        expired = DateTime.Now + _lifetime;
                    }
                return data;
            }
        }
    }
}
