using System.Runtime.CompilerServices;

namespace RZ.Foundation.Extensions;

public static class StringExtension
{
    extension(string s)
    {
        [PublicAPI, Pure]
        public bool iEquals(string other) => string.Equals(s, other, StringComparison.OrdinalIgnoreCase);

        [PublicAPI, Pure]
        public string Limit(int maxLength)
            => s[..Math.Min(Math.Max(0, maxLength), s.Length)];

        [PublicAPI, Pure]
        public string Truncate(int n)
            => n < 1 ? string.Empty : s.Length <= n ? s : string.Concat(s.AsSpan(0, n - 1), "…");

        [PublicAPI, Pure]
        public string TruncateRight(int n)
        => n <= 0? string.Empty : n >= s.Length? s : string.Concat("…", s.AsSpan(s.Length - n + 1));

        [PublicAPI, Pure]
        public Option<DateTime> ToDateTime() => DateTime.TryParse(s, out var dt) ? dt : None;

        [PublicAPI, Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string Left(int n) => s.Limit(n);

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