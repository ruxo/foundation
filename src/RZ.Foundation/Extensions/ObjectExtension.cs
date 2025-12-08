using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace RZ.Foundation.Extensions;

[PublicAPI]
public static class ObjectExtension
{
    public static T[] AsArray<T>(this IEnumerable<T> iter) => iter as T[] ?? iter.ToArray();

    extension<T>(T source)
    {
        [PublicAPI, Pure]
        public bool IsEither(T v1, T v2) {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(source, v1) || comparer.Equals(source, v2);
        }

        [PublicAPI, Pure]
        public bool IsEither(T v1, T v2, T v3) {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(source, v1) || comparer.Equals(source, v2) || comparer.Equals(source, v3);
        }

        [PublicAPI, Pure]
        public bool IsEither(T v1, T v2, T v3, T v4) {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(source, v1) || comparer.Equals(source, v2) || comparer.Equals(source, v3) || comparer.Equals(source, v4);
        }

        [PublicAPI, Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Unit Ignore() => Unit.Default;

        [PublicAPI, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Void() { }
    }
}