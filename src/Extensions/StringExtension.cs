using System;
using System.Collections.Generic;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Extensions
{
    public static class StringExtension
    {
        public static bool iEquals(this string s, string other) => string.Equals(s, other, StringComparison.OrdinalIgnoreCase);
        public static string Limit(this string s, int maxLength) => s.Substring(0, Math.Min(Math.Max(0, maxLength), s.Length));

        public static string Join(this IEnumerable<string> sseq, char delimiter) =>
#if NET472 || NETSTANDARD2_0
            string.Join(delimiter.ToString(), sseq);
#else
            string.Join(delimiter, sseq);
#endif
        public static string Join(this IEnumerable<string> sseq, string delimiter) => string.Join(delimiter, sseq);

        public static Option<DateTime> ToDateTime(this string s) => DateTime.TryParse(s, out var dt) ? dt : None<DateTime>();
    }
}