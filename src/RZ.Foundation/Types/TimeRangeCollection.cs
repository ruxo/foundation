using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace RZ.Foundation.Types;

[PublicAPI]
public static class TimeRangeCollection
{
    extension(IEnumerable<TimeRange> self)
    {
        public TimeRange? FindContainer(TimeRange other)
            => self.Find(x => x.Contains(other)).ToNullable();

        public TimeRange? FindOverlapped(TimeRange other)
            => self.Find(x => x.IsOverlapped(other)).ToNullable();

        public IEnumerable<TimeRange> Exclude(TimeRange other)
            => self.SelectMany(x => x.Exclude(other));

        public IEnumerable<TimeRange> Exclude(IEnumerable<TimeRange> others)
            => others.Aggregate(self, (current, tr) => current.Exclude(tr).ToArray());
    }
}