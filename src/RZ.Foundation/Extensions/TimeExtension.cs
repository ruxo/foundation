using System;

namespace RZ.Foundation.Extensions
{
    public static class TimeExtension
    {
        #region Time units

        public static TimeSpan Miliseconds(this int ms) => TimeSpan.FromMilliseconds(ms);
        public static TimeSpan Seconds(this int s) => TimeSpan.FromSeconds(s);
        public static TimeSpan Minutes(this int m) => TimeSpan.FromMinutes(m);
        public static TimeSpan Hours(this int h) => TimeSpan.FromHours(h);
        public static TimeSpan Days(this int d) => TimeSpan.FromDays(d);

        public static TimeSpan Miliseconds(this double ms) => TimeSpan.FromMilliseconds(ms);
        public static TimeSpan Seconds(this double s) => TimeSpan.FromSeconds(s);
        public static TimeSpan Minutes(this double m) => TimeSpan.FromMinutes(m);
        public static TimeSpan Hours(this double h) => TimeSpan.FromHours(h);
        public static TimeSpan Days(this double d) => TimeSpan.FromDays(d);

        #endregion

#if NETSTANDARD2_0
        public static DateTimeOffset ToTimeZone(this DateTime datetime, TimeZoneInfo tz) =>
            new DateTimeOffset(TimeZoneInfo.ConvertTimeFromUtc(datetime.ToUniversalTime(), tz), tz.BaseUtcOffset);
#endif

        public static DateTimeOffset Midnight(this DateTimeOffset anytime) => anytime - anytime.TimeOfDay;
    }
}