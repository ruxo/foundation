using System.Collections.Generic;
using System.Linq;

namespace RZ.Foundation.Extensions
{
    public static class ObjectExtension
    {
        public static T[] AsArray<T>(this IEnumerable<T> iter) => iter as T[] ?? iter.ToArray();

        public static bool IsEither<T>(this T source, T v1, T v2) {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(source, v1) || comparer.Equals(source, v2);
        }

        public static bool IsEither<T>(this T source, T v1, T v2, T v3) {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(source, v1) || comparer.Equals(source, v2) || comparer.Equals(source, v3);
        }

        public static bool IsEither<T>(this T source, T v1, T v2, T v3, T v4) {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(source, v1) || comparer.Equals(source, v2) || comparer.Equals(source, v3) || comparer.Equals(source, v4);
        }
    }
}