using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using Newtonsoft.Json;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Extensions
{
    public static class ObjectExtension
    {
        public static T[] AsArray<T>(this IEnumerable<T> iter) => iter as T[] ?? iter.ToArray();

        public static Result<string> AsJson<T>(this T data) => Try(() => JsonConvert.SerializeObject(data)).Try();

        public static Result<T> DeserializeJson<T>(this string s) => Try(() => JsonConvert.DeserializeObject<T>(s)).Try();

        public static bool IsEither<T>(this T source, T v1, T v2) {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(source, v1) || comparer.Equals(source, v2);
        }

        public static bool IsEither<T>(this T source, T v1, T v2, T v3) {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(source, v1) || comparer.Equals(source, v2) || comparer.Equals(source, v3);
        }
    }
}