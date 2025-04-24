using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using LanguageExt.UnitsOfMeasure;

namespace RZ.Foundation.Types;

public readonly struct TimeRange : IEquatable<TimeRange>
{
    /// <summary>
    /// Inclusive begin time
    /// </summary>
    public TimeSpan? Begin { get; }

    /// <summary>
    /// Exclusive end time
    /// </summary>
    public TimeSpan? End { get; }

    [JsonIgnore]
    public TimeSpan Duration => NullAsMax(End) - NullAsMin(Begin);

    public static readonly TimeRange Empty = new(TimeSpan.MinValue, TimeSpan.MinValue);

    [JsonConstructor]
    public TimeRange(TimeSpan? begin = null, TimeSpan? end = null)
    {
        if (end <= begin)
            Begin = End = TimeSpan.MinValue;
        else{
            Begin = begin;
            End = end;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeRange Create(TimeSpan? begin = null, TimeSpan? end = null) => new(begin, end);

    [JsonIgnore]
    public bool IsNoLimit => Begin == null && End == null;

    [JsonIgnore]
    public bool IsEmpty => !IsNoLimit && Begin == End;

    public bool Contains(TimeSpan d) => NullAsMin(Begin) <= d && d <= NullAsMax(End);

    public bool Contains(TimeRange other)
        => (Begin == null || Begin <= other.Begin) && (End == null || End >= other.End);

    public TimeRange Intersect(TimeRange other)
    {
        if (IsEmpty || other.IsEmpty)
            return Empty;
        var begin = NullAsMin(Begin) > NullAsMin(other.Begin) ? Begin : other.Begin;
        var end = NullAsMax(End) < NullAsMax(other.End) ? End : other.End;

        return new TimeRange(begin, end);
    }

    public bool IsOverlapped(TimeRange other)
        => Intersect(other) != Empty;

    /// <summary>
    /// Merge another time range
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public IEnumerable<TimeRange> Include(TimeRange other) {
        if (Contains(other))
            yield return this;
        else if (other.Contains(this))
            yield return other;
        else if (Intersect(other).IsEmpty && Begin != other.End && End != other.Begin){
            yield return this;
            yield return other;
        } else{
            var begin = NullAsMin(Begin) < NullAsMin(other.Begin) ? Begin : other.Begin;
            var end = NullAsMax(End) > NullAsMax(other.End) ? End : other.End;
            yield return Create(begin, end);
        }
    }

    public IEnumerable<TimeRange> Exclude(TimeRange other) {
        var common = Intersect(other);
        if (common.IsEmpty)
            yield return this;
        else if (common == this)
        {}
        else if (common == other){
            if (NullAsMin(Begin) < NullAsMin(other.Begin))
                yield return new TimeRange(Begin, other.Begin);
            if (NullAsMax(other.End) < NullAsMax(End))
                yield return new TimeRange(other.End, End);
        }
        else{
            var begin = NullAsMin(Begin) < NullAsMin(common.Begin) ? Begin : common.End;
            var end = NullAsMax(End) > NullAsMax(common.End) ? End : common.Begin;
            yield return Create(begin, end);
        }
    }

    static TimeSpan NullAsMin(TimeSpan? d) => d ?? TimeSpan.MinValue;
    static TimeSpan NullAsMax(TimeSpan? d) => d ?? TimeSpan.MaxValue;

    #region Equality

    public static bool operator ==(TimeRange a, TimeRange b) => a.Equals(b);
    public static bool operator !=(TimeRange a, TimeRange b) => !a.Equals(b);

    public bool Equals(TimeRange other)
        => Nullable.Equals(Begin, other.Begin) && Nullable.Equals(End, other.End);

    public override bool Equals(object? obj)
        => obj is TimeRange range && this == range;

    public override int GetHashCode() {
        unchecked {
            return ((Begin?.GetHashCode() ?? 0) * 397) ^ (End?.GetHashCode() ?? 0);
        }
    }

    #endregion

    public override string ToString()
        => Begin == null && End == null
               ? "[*]"
               : IsEmpty ? "[]" : $"[{DisplayTime(Begin)} - {DisplayTime(End)}]";

    public static TimeRange Parse(string s)
        => TryParse(s).IfNone(() => throw ExceptionExtension.CreateError($"Unrecognized TimeRange format: {s}", "invalid_request", nameof(TimeRange), s));

    public static Option<TimeRange> TryParse(string s) {
        var trimmed = s.Trim('[',']',' ');
        if (trimmed == "*")
            return new TimeRange();
        if (string.IsNullOrEmpty(trimmed))
            return Empty;

        var timeParts = trimmed.Split('-').Select(part => part.Trim()).ToArray();
        return timeParts.Length == 2 ? Create(ParsePart(timeParts[0]), ParsePart(timeParts[1])) : None;
    }

    static TimeSpan? ParsePart(string p)
        => p == "*"
               ? null
               : TimeSpan.TryParse(p, out var v)
                   ? v
                   : throw new InvalidOperationException($"Invalid time part: {p}").AddMetaData("invalid_request", nameof(TimeRange), p);

    static string DisplayTime(TimeSpan? t)
        => (t >= 1.Days()? t.Value.ToString(@"d\:hh\:mm") : t?.ToString(@"hh\:mm")) ?? "*";
}