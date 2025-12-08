using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace RZ.Foundation.Extensions;

public static class StringExtension
{
    extension(string s)
    {
        [PublicAPI, Pure]
        public bool iEquals(string other) => string.Equals(s, other, StringComparison.OrdinalIgnoreCase);

        [PublicAPI, Pure]
        public string Limit(int maxLength) => s.Substring(0, Math.Min(Math.Max(0, maxLength), s.Length));

        [PublicAPI, Pure]
        public Option<DateTime> ToDateTime() => DateTime.TryParse(s, out var dt) ? dt : None;

        [PublicAPI, Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Left(int n) => s[..Math.Min(s.Length, n)];

        [PublicAPI, Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Right(int n) {
            return s[Math.Max(0, s.Length - n)..];
        }

        [PublicAPI, Pure]
        public string? NotEmpty() => string.IsNullOrEmpty(s) ? null : s;

        [PublicAPI, Pure]
        public string? NotWhiteSpace() => string.IsNullOrWhiteSpace(s) ? null : s;
    }

    extension(IEnumerable<string> sseq)
    {
        [PublicAPI, Pure]
        public string Join(char delimiter) => string.Join(delimiter.ToString(), sseq);

        [PublicAPI, Pure]
        public string Join(string delimiter) => string.Join(delimiter, sseq);
    }
}