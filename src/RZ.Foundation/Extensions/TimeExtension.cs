using System;
using JetBrains.Annotations;

namespace RZ.Foundation.Extensions;

[PublicAPI]
public static class TimeExtension
{
    public static DateTimeOffset ToTimeZone(this DateTime datetime, TimeZoneInfo tz) =>
        new(TimeZoneInfo.ConvertTimeFromUtc(datetime.ToUniversalTime(), tz), tz.BaseUtcOffset);

    public static DateTimeOffset Midnight(this DateTimeOffset anytime) => anytime - anytime.TimeOfDay;
}