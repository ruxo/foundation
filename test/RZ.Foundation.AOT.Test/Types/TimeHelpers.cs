global using static RZ.Foundation.Types.TimeHelpers;
using LanguageExt.UnitsOfMeasure;

namespace RZ.Foundation.Types;

public static class TimeHelpers
{
    public static TimeRange T(int from, int to) => new(from.Hours(), to.Hours());
}
