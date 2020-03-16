using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace RZ.Foundation.Extensions
{
    public static class NullableExtension
    {
        public static V Try<T, V>(this T v, Func<T, V> mapper) where T : class? where V : class? => v != null ? mapper(v) : null;

        public static async Task<V> TryAsync<T, V>(this T v, Func<T, Task<V>> mapper) where T : class? where V : class? =>
            v == null ? null : await mapper(v);

        public static V? Try<T, V>(this T? v, Func<T, V> mapper) where T : struct where V : struct => v.HasValue ? (V?) mapper(v.Value) : null;
        public static V? Try<T, V>(this T? v, Func<T, V?> mapper) where T : struct where V : struct => v.HasValue ? mapper(v.Value) : null;

        public static async Task<V?> TryAsync<T, V>(this T? v, Func<T, Task<V>> mapper) where T : struct where V : struct =>
            v.HasValue ? (V?) await mapper(v.Value) : null;
        public static Task<V?> TryAsync<T, V>(this T? v, Func<T, Task<V?>> mapper) where T : struct where V : struct =>
            v.HasValue ? mapper(v.Value) : Task.FromResult<V?>(null);

        public static T Where<T>(this T v, Predicate<T> predicate) where T : class? => predicate(v) ? v : null;
        public static T? Where<T>(this T? v, Predicate<T?> predicate) where T : struct => predicate(v) ? v : null;
    }

    public static class NullableExtension2
    {
        public static V? Try<T, V>(this T v, Func<T, V> mapper) where T : class? where V : struct => v != null ? (V?) mapper(v) : null;
        public static async Task<V?> TryAsync<T, V>(this T v, Func<T, Task<V>> mapper) where T : class? where V : struct =>
            v != null ? (V?) await mapper(v) : null;

        [return: MaybeNull]
        public static V Try<T, V>(this T? v, Func<T, V> mapper) where T : struct where V : class? => v.HasValue ? mapper(v.Value) : null;
        public static async Task<V> Try<T, V>(this T? v, Func<T, Task<V>> mapper) where T : struct where V : class? =>
            v.HasValue ? await mapper(v.Value) : null;
    }

    public static class NullableExtension3
    {
        public static V? Try<T, V>(this T v, Func<T, V?> mapper) where T : class? where V : struct => v != null ? mapper(v) : null;
        public static async Task<V?> TryAsync<T, V>(this T v, Func<T, Task<V?>> mapper) where T : class? where V : struct =>
            v != null ? await mapper(v) : null;
    }
}