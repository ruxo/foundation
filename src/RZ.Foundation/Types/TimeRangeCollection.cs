using System.Collections.Generic;
using System.Linq;

namespace RZ.Foundation.Types;

public static class TimeRangeCollection
{
    public static TimeRange? FindOverlapped(this IEnumerable<TimeRange> self, TimeRange other)
        => self.Find(x => x.IsOverlapped(other)).ToNullable();

    public static IEnumerable<TimeRange> Exclude(this IEnumerable<TimeRange> self, TimeRange other)
        => self.SelectMany(x => x.Exclude(other));

    public static IEnumerable<TimeRange> Exclude(this IEnumerable<TimeRange> self, IEnumerable<TimeRange> others)
        => others.Aggregate(self, (current, tr) => current.Exclude(tr).ToArray());
}