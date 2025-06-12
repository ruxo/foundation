using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RZ.Foundation.Extensions;

public static class StringExtension
{
    public static bool iEquals(this string s, string other) => string.Equals(s, other, StringComparison.OrdinalIgnoreCase);
    public static string Limit(this string s, int maxLength) => s.Substring(0, Math.Min(Math.Max(0, maxLength), s.Length));

    public static string Join(this IEnumerable<string> sseq, char delimiter) => string.Join(delimiter.ToString(), sseq);
    public static string Join(this IEnumerable<string> sseq, string delimiter) => string.Join(delimiter, sseq);

    public static Option<DateTime> ToDateTime(this string s) => DateTime.TryParse(s, out var dt) ? dt : None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Left(this string s, int n) => s[..Math.Min(s.Length, n)];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Right(this string s, int n) {
        return s[Math.Max(0, s.Length - n)..];
    }

    public static string? NotEmpty(this string s) => string.IsNullOrEmpty(s)? null : s;
    public static string? NotWhiteSpace(this string s) => string.IsNullOrWhiteSpace(s)? null : s;
}