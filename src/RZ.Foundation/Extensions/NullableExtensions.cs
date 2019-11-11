using System;

namespace RZ.Foundation.Extensions
{
    public static class NullableExtension
    {
        public static V? Try<T, V>(this T? v, Func<T, V> mapper) where T : class where V : class => v != null ? mapper(v) : null;
        public static V? Try<T, V>(this T? v, Func<T, V> mapper) where T : struct where V : struct => v.HasValue ? (V?) mapper(v.Value) : null;
        public static V? Try<T, V>(this T? v, Func<T, V?> mapper) where T : struct where V : struct => v.HasValue ? mapper(v.Value) : null;
    }

    public static class NullableExtension2
    {
        public static V? Try<T, V>(this T? v, Func<T, V> mapper) where T : class where V : struct => v != null ? (V?) mapper(v) : null;
        public static V? Try<T, V>(this T? v, Func<T, V> mapper) where T : struct where V : class => v.HasValue ? mapper(v.Value) : null;
    }
}