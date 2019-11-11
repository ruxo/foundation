﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RZ.Foundation.Extensions
{
    public static class ObjectExtension
    {
        public static T[] AsArray<T>(this IEnumerable<T> iter) => iter as T[] ?? iter.ToArray();

        public static ApiResult<string> AsJson<T>(this T data) => ApiResult<string>.SafeCall(() => JsonConvert.SerializeObject(data));

        public static ApiResult<T> DeserializeJson<T>(this string s) => ApiResult<T>.SafeCall(() => JsonConvert.DeserializeObject<T>(s));

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