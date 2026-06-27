namespace RZ.Foundation.Types;

public sealed class TimeRangeCollectionTest
{
    static readonly TimeRange MorningWorkHours = T(8, 12);
    static readonly TimeRange AfternoonWorkHours = T(13, 17);
    static readonly TimeRange[] WorkHours = [MorningWorkHours, AfternoonWorkHours];

    public static IEnumerable<Func<(TimeRange[], TimeRange, TimeRange?)>> OverlappedCases() => [
        () => (WorkHours, T(11,13), MorningWorkHours),
        () => (WorkHours, T(16,18), AfternoonWorkHours),
        () => (WorkHours, T(12,13), null)
    ];

    [Test]
    [MethodDataSource(nameof(OverlappedCases))]
    public async Task OverlappedFound(TimeRange[] source, TimeRange target, TimeRange? expected) {
        await Assert.That(source.FindOverlapped(target)).IsEqualTo(expected);
    }

    public static IEnumerable<Func<(TimeRange[], TimeRange, TimeRange[])>> ExclusionCases() => [
        () => (WorkHours, T(11,14), [T(8,11), T(14,17)]),
        () => (WorkHours, T(12,13), WorkHours)
    ];

    [Test]
    [MethodDataSource(nameof(ExclusionCases))]
    public async Task ExclusionTests(TimeRange[] source, TimeRange target, TimeRange[] expected) {
        await Assert.That(source.Exclude(target)).IsEquivalentTo(expected, EqualityComparer<TimeRange>.Default);
    }

    public static IEnumerable<Func<(TimeRange[], TimeRange[], TimeRange[])>> SetExclusionCases() => [
        () => (WorkHours, [T(7,10), T(11,14)], [T(10,11), T(14,17)]),
        () => (WorkHours, [T(5,6), T(12,13), T(18,19)], WorkHours),
        () => (WorkHours, [AfternoonWorkHours], [MorningWorkHours])
    ];

    [Test]
    [MethodDataSource(nameof(SetExclusionCases))]
    public async Task SetExclusionTests(TimeRange[] source, TimeRange[] targets, TimeRange[] expected) {
        await Assert.That(source.Exclude(targets)).IsEquivalentTo(expected, EqualityComparer<TimeRange>.Default);
    }
}
