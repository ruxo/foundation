using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace RZ.Foundation.Types;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TimeRangeCollectionTest
{
    static readonly TimeRange MorningWorkHours = T(8, 12);
    static readonly TimeRange AfternoonWorkHours = T(13, 17);
    static readonly TimeRange[] WorkHours = [MorningWorkHours, AfternoonWorkHours];

    public static readonly TheoryData<TimeRange[], TimeRange, TimeRange?> OverlappedCases = new()
    {
        {WorkHours, T(11,13), MorningWorkHours},
        {WorkHours, T(16,18), AfternoonWorkHours},
        {WorkHours, T(12,13), null}
    };

    [Theory]
    [MemberData(nameof(OverlappedCases))]
    public void OverlappedFound(TimeRange[] source, TimeRange target, TimeRange? expected) {
        source.FindOverlapped(target).Should().Be(expected);
    }

    public static readonly TheoryData<TimeRange[], TimeRange, TimeRange[]> ExclusionCases = new()
    {
        {WorkHours, T(11,14), [T(8,11), T(14,17)]},
        {WorkHours, T(12,13), WorkHours}
    };

    [Theory]
    [MemberData(nameof(ExclusionCases))]
    public void ExclusionTests(TimeRange[] source, TimeRange target, TimeRange[] expected) {
        source.Exclude(target).Should().BeEquivalentTo(expected);
    }

    public static readonly TheoryData<TimeRange[], TimeRange[], TimeRange[]> SetExclusionCases = new()
    {
        {WorkHours, [T(7,10), T(11,14)], [T(10,11), T(14,17)]},
        {WorkHours, [T(5,6), T(12,13), T(18,19)], WorkHours},
        {WorkHours, [AfternoonWorkHours], [MorningWorkHours]}
    };

    [Theory]
    [MemberData(nameof(SetExclusionCases))]
    public void SetExclusionTests(TimeRange[] source, TimeRange[] targets, TimeRange[] expected) {
        source.Exclude(targets).Should().BeEquivalentTo(expected);
    }
}