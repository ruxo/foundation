using System;

namespace RZ.Foundation.Types
{
    public struct DateRange
    {
        public DateTime? Begin { get; }
        public DateTime? End { get; }

        public static readonly DateRange Empty = new (DateTime.MinValue, DateTime.MinValue);

        public DateRange(DateTime? begin, DateTime? end)
        {
            if (end < begin)
                Begin = End = DateTime.MinValue;
            else {
                Begin = begin;
                End = end;
            }
        }

        public bool IsNoLimit => Begin == null && End == null;
        public bool IsEmpty => !IsNoLimit && Begin == End;

        public bool Contains(DateTime d) => NullAsMin(Begin) <= d && d <= NullAsMax(End);

        public DateRange Intersect(DateRange other)
        {
            if (IsEmpty || other.IsEmpty)
                return Empty;
            var begin = NullAsMin(Begin) > NullAsMin(other.Begin) ? Begin : other.Begin;
            var end = NullAsMax(End) < NullAsMax(other.End) ? End : other.End;

            return new(begin, end);
        }

        static DateTime NullAsMin(DateTime? d) => d ?? DateTime.MinValue;
        static DateTime NullAsMax(DateTime? d) => d ?? DateTime.MaxValue;
    }
}