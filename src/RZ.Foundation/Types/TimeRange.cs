using System;
using System.Linq;
using LanguageExt;
using RZ.Foundation.Extensions;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Types
{
    public readonly struct TimeRange
    {
        public TimeSpan? Begin { get; }
        public TimeSpan? End { get; }

        public static readonly TimeRange Empty = new TimeRange(TimeSpan.MinValue, TimeSpan.MinValue);

        public TimeRange(TimeSpan? begin, TimeSpan? end)
        {
            if (begin != null && end != null && end < begin)
            {
                Begin = End = TimeSpan.MinValue;
            }
            else
            {
                Begin = begin;
                End = end;
            }
        }
        public static TimeRange Create(TimeSpan? begin, TimeSpan? end) => new TimeRange(begin, end);

        public bool IsNoLimit => Begin == null && End == null;
        public bool IsEmpty => !IsNoLimit && Begin == End;

        public bool Contains(TimeSpan d) => NullAsMin(Begin) <= d && d <= NullAsMax(End);

        public TimeRange Intersect(TimeRange other)
        {
            if (IsEmpty || other.IsEmpty)
                return Empty;
            var begin = NullAsMin(Begin) > NullAsMin(other.Begin) ? Begin : other.Begin;
            var end = NullAsMax(End) < NullAsMax(other.End) ? End : other.End;

            return new TimeRange(begin, end);
        }

        static TimeSpan NullAsMin(TimeSpan? d) => d ?? TimeSpan.MinValue;
        static TimeSpan NullAsMax(TimeSpan? d) => d ?? TimeSpan.MaxValue;

        #region Equality

        public static bool operator ==(TimeRange a, TimeRange b) => a.Begin == b.Begin && a.End == b.End;
        public static bool operator !=(TimeRange a, TimeRange b) => a.Begin != b.Begin || a.End != b.End;

        public override bool Equals(object? obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TimeRange range && this == range;
        }

        public override int GetHashCode() {
            unchecked {
                return ((Begin?.GetHashCode() ?? 0) * 397) ^ (End?.GetHashCode() ?? 0);
            }
        }

        #endregion

        public override string ToString() => Begin == null && End == null ? "*" : $"{DisplayTime(Begin)} - {DisplayTime(End)}";

        public static TimeRange Parse(string s) =>
            TryParse(s).IfNone(() => throw ExceptionExtension.CreateError($"Unrecognized TimeRange format: {s}", "invalid_request", nameof(TimeRange), s));

        public static Option<TimeRange> TryParse(string s) {
            if (s == "*")
                return new TimeRange();

            var timeParts = s.Split('-').Select(part => part.Trim()).ToArray();
            return timeParts.Length == 2 ? Some(Create(ParsePart(timeParts[0]), ParsePart(timeParts[1]))) : None;
        }

        static TimeSpan? ParsePart(string p) =>
            p == "*" ? (TimeSpan?) null
                     : TimeSpan.TryParse(p, out var v)
                     ? v : throw new InvalidOperationException($"Invalid time part: {p}").AddMetaData("invalid_request", nameof(TimeRange), p);

        static string DisplayTime(TimeSpan? t) => t?.ToString(@"hh\:mm") ?? "*";
    }
}