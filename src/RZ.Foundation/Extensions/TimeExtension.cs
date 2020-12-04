using System;

namespace RZ.Foundation.Extensions
{
    public static class TimeExtension
    {
        public static DateTimeOffset ToTimeZone(this DateTime datetime, TimeZoneInfo tz) =>
            new(TimeZoneInfo.ConvertTimeFromUtc(datetime.ToUniversalTime(), tz), tz.BaseUtcOffset);

        public static DateTimeOffset Midnight(this DateTimeOffset anytime) => anytime - anytime.TimeOfDay;
    }
}