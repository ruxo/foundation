using LanguageExt.UnitsOfMeasure;

namespace RZ.Foundation.Types;

public sealed class TimeRangeTest
{
    [Test]
    [DisplayName("Test emptiness of TimeRange")]
    public async Task EmptyIsEmpty() {
        await Assert.That(new TimeRange(8.Hours(), end: null)).IsNotEqualTo(TimeRange.Empty);
        await Assert.That(TimeRange.Empty).IsEqualTo(TimeRange.Empty);
        await Assert.That(TimeRange.Empty.IsEmpty).IsTrue();
    }

    [Test]
    public async Task InvalidTimeRangeIsEmpty() {
        var x = new TimeRange(8.Hours(), 7.Hours());

        await Assert.That(x).IsEqualTo(TimeRange.Empty);
    }

    static readonly TimeRange WorkHours = T(8, 17);
    static readonly TimeRange LunchTime = T(12, 13);
    static readonly TimeRange InfiniteDays = default;
    static readonly TimeRange Before6 = new(null, 18.Hours());
    static readonly TimeRange Hours24 = new(TimeSpan.Zero, 24.Hours());

    #region Contains tests

    [Test]
    [DisplayName("Lunch time is in work hours")]
    public async Task LunchTimeIsInWorkHours() {
        await Assert.That(WorkHours.Contains(LunchTime.Begin!.Value)).IsTrue();
        await Assert.That(WorkHours.Contains(LunchTime.End!.Value)).IsTrue();
        await Assert.That(WorkHours.Contains(LunchTime)).IsTrue();
    }

    public static readonly TimeRange[] AllCases = [WorkHours, LunchTime, InfiniteDays, Before6, Hours24];

    public static IEnumerable<(TimeRange, bool)> GenerateCase(Func<TimeRange, bool> predicate)
        => AllCases.Select(@case => (@case, predicate(@case)));

    public static IEnumerable<(TimeRange, bool)> InfiniteDaysContainsAll() => GenerateCase(_ => true);

    [Test]
    [MethodDataSource(nameof(InfiniteDaysContainsAll))]
    public async Task InfiniteDaysContainsAllCases(TimeRange @case, bool expected) {
        await Assert.That(InfiniteDays.Contains(@case)).IsEqualTo(expected);
    }

    public static IEnumerable<(TimeRange, bool)> WorkHoursContainsTestData() => GenerateCase(x => x == WorkHours || x == LunchTime);

    [Test]
    [MethodDataSource(nameof(WorkHoursContainsTestData))]
    public async Task WorkHoursContainsAllCases(TimeRange @case, bool expected) {
        await Assert.That(WorkHours.Contains(@case)).IsEqualTo(expected);
    }

    #endregion

    #region Intersect tests

    public static IEnumerable<TimeRange> AllIntersectCases() => AllCases;

    [Test]
    [MethodDataSource(nameof(AllIntersectCases))]
    public async Task IntersectWithEmptyIsEmpty(TimeRange tr) {
        await Assert.That(tr.Intersect(TimeRange.Empty)).IsEqualTo(TimeRange.Empty);
        await Assert.That(TimeRange.Empty.Intersect(tr)).IsEqualTo(TimeRange.Empty);
    }

    public static IEnumerable<(TimeRange, TimeRange, TimeRange)> IntersectCases() => [
        (WorkHours, LunchTime, LunchTime),
        (LunchTime, WorkHours, LunchTime),
        (WorkHours, Hours24, WorkHours),
        (LunchTime, Before6, LunchTime),
        (InfiniteDays, Hours24, Hours24),
        (Before6, Hours24, new TimeRange(TimeSpan.Zero, 18.Hours()))
    ];

    [Test]
    [MethodDataSource(nameof(IntersectCases))]
    public async Task IntersectCasesTest(TimeRange a, TimeRange b, TimeRange expected) {
        await Assert.That(a.Intersect(b)).IsEqualTo(expected);
    }

    #endregion

    #region Exclusion tests

    public static IEnumerable<Func<(TimeRange, TimeRange, TimeRange[])>> ExclusionCases() => [
        () => (WorkHours, LunchTime, [T(8,12), T(13,17)]),
        () => (WorkHours, T(17,18), [WorkHours]),
        () => (WorkHours, T(7,8), [WorkHours]),
        () => (WorkHours, T(20,21), [WorkHours]),
        () => (WorkHours, WorkHours, []),
        () => (LunchTime, WorkHours, []),
        () => (T(12,14), LunchTime, [T(13,14)]),
        () => (T(12,14), T(13,14), [LunchTime]),
        () => (WorkHours, Hours24, []),
        () => (LunchTime, Before6, []),
        () => (InfiniteDays, Hours24, [new TimeRange(end: TimeSpan.Zero), new TimeRange(24.Hours())]),
        () => (Before6, Hours24, [new TimeRange(end: TimeSpan.Zero)])
    ];

    [Test]
    [MethodDataSource(nameof(ExclusionCases))]
    public async Task ExclusionCasesTest(TimeRange a, TimeRange b, TimeRange[] expected) {
        await Assert.That(a.Exclude(b)).IsEquivalentTo(expected, EqualityComparer<TimeRange>.Default);
    }

    #endregion

    #region Inclusion tests

    public static IEnumerable<Func<(TimeRange, TimeRange, TimeRange[])>> InclusionCases() => [
        () => (WorkHours, LunchTime, [WorkHours]),
        () => (LunchTime, WorkHours, [WorkHours]),
        () => (WorkHours, Hours24, [Hours24]),
        () => (LunchTime, Before6, [Before6]),
        () => (InfiniteDays, Hours24, [InfiniteDays]),
        () => (Before6, Hours24, [new TimeRange(end: 1.Days())]),
        () => (LunchTime, new TimeRange(12.Hours() + 30.Minutes(), 14.Hours()), [T(12,14)]),
        () => (new TimeRange(12.Hours(), 13.Hours() + 30.Minutes()), T(13, 14), [T(12,14)]),
        () => (LunchTime, T(13, 14), [T(12,14)])
    ];

    [Test]
    [MethodDataSource(nameof(InclusionCases))]
    public async Task InclusionCasesTest(TimeRange a, TimeRange b, TimeRange[] expected) {
        await Assert.That(a.Include(b)).IsEquivalentTo(expected, EqualityComparer<TimeRange>.Default);
    }

    #endregion

    public static IEnumerable<(string, TimeRange)> ParseCases() => [
        ("[*]", new TimeRange()),
        ("[]", TimeRange.Empty),
        ("[* - 10:00]", new TimeRange(end: 10.Hours())),
        ("[15:00 - *]", new TimeRange(15.Hours())),
        ("[10:00 - 15:00]", new TimeRange(10.Hours(), 15.Hours())),
        ("[* - *]", new TimeRange())
    ];

    [Test]
    [MethodDataSource(nameof(ParseCases))]
    public async Task ParseInfiniteDays(string pattern, TimeRange expected) {
        await Assert.That(TimeRange.Parse(pattern)).IsEqualTo(expected);
    }
}
