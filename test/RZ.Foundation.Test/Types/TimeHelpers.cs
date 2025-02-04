global using static RZ.Foundation.Types.TimeHelpers;
using FluentAssertions.Extensions;

namespace RZ.Foundation.Types;

public static class TimeHelpers
{
    public static TimeRange T(int from, int to) => new(from.Hours(), to.Hours());
}