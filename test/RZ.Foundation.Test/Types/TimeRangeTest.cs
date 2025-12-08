using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using JetBrains.Annotations;
using Xunit;

namespace RZ.Foundation.Types;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TimeRangeTest
{
    [Fact(DisplayName = "Test emptiness of TimeRange")]
    public void EmptyIsEmpty() {
        new TimeRange(8.Hours(), end: null).Should().NotBe(TimeRange.Empty);
        TimeRange.Empty.Should().Be(TimeRange.Empty);
        TimeRange.Empty.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void InvalidTimeRangeIsEmpty() {
        var x = new TimeRange(8.Hours(), 7.Hours());

        x.Should().Be(TimeRange.Empty);
    }

    static readonly TimeRange WorkHours = T(8, 17);
    static readonly TimeRange LunchTime = T(12, 13);
    static readonly TimeRange InfiniteDays = default;
    static readonly TimeRange Before6 = new(null, 18.Hours());
    static readonly TimeRange Hours24 = new(TimeSpan.Zero, 24.Hours());

    #region Contains tests

    [Fact(DisplayName = "Lunch time is in work hours")]
    public void LunchTimeIsInWorkHours() {
        WorkHours.Contains(LunchTime.Begin!.Value).Should().BeTrue();
        WorkHours.Contains(LunchTime.End!.Value).Should().BeTrue();
        WorkHours.Contains(LunchTime).Should().BeTrue();
    }

    public static readonly TimeRange[] AllCases = [WorkHours, LunchTime, InfiniteDays, Before6, Hours24];

    public static TheoryData<TimeRange, bool> GenerateCase(Func<TimeRange, bool> predicate)
        => new(from @case in AllCases
               let expected = predicate(@case)
               select (@case, expected));

    public static readonly TheoryData<TimeRange, bool> InfiniteDaysContainsAll = GenerateCase(_ => true);

    [Theory]
    [MemberData(nameof(InfiniteDaysContainsAll))]
    public void InfiniteDaysContainsAllCases(TimeRange @case, bool expected) {
        InfiniteDays.Contains(@case).Should().Be(expected);
    }

    public static readonly TheoryData<TimeRange, bool> WorkHoursContainsTestData = GenerateCase(x => x == WorkHours || x == LunchTime);

    [Theory]
    [MemberData(nameof(WorkHoursContainsTestData))]
    public void WorkHoursContainsAllCases(TimeRange @case, bool expected) {
        WorkHours.Contains(@case).Should().Be(expected);
    }

    #endregion

    #region Intersect tests

    public static readonly IEnumerable<object?[]> AllIntersectCases = AllCases.Select(v => new object[] { v }).ToArray();

    [Theory]
    [MemberData(nameof(AllIntersectCases))]
    public void IntersectWithEmptyIsEmpty(TimeRange tr) {
        tr.Intersect(TimeRange.Empty).Should().Be(TimeRange.Empty);
        TimeRange.Empty.Intersect(tr).Should().Be(TimeRange.Empty);
    }

    public static readonly TheoryData<TimeRange, TimeRange, TimeRange> IntersectCases = new() {
        { WorkHours, LunchTime, LunchTime },
        { LunchTime, WorkHours, LunchTime },
        { WorkHours, Hours24, WorkHours },
        { LunchTime, Before6, LunchTime },
        { InfiniteDays, Hours24, Hours24 },
        { Before6, Hours24, new(TimeSpan.Zero, 18.Hours()) }
    };

    [Theory]
    [MemberData(nameof(IntersectCases))]
    public void IntersectCasesTest(TimeRange a, TimeRange b, TimeRange expected) {
        a.Intersect(b).Should().Be(expected);
    }

    #endregion

    #region Exclusion tests

    public static readonly TheoryData<TimeRange, TimeRange, TimeRange[]> ExclusionCases = new() {
        { WorkHours, LunchTime, [T(8,12), T(13,17)] },
        { WorkHours, T(17,18), [WorkHours] },
        { WorkHours, T(7,8), [WorkHours] },
        { WorkHours, T(20,21), [WorkHours] },
        { WorkHours, WorkHours, [] },
        { LunchTime, WorkHours, [] },
        { T(12,14), LunchTime, [T(13,14)] },
        { T(12,14), T(13,14), [LunchTime] },
        { WorkHours, Hours24, [] },
        { LunchTime, Before6, [] },
        { InfiniteDays, Hours24, [new TimeRange(end: TimeSpan.Zero), new TimeRange(24.Hours())] },
        { Before6, Hours24, [new TimeRange(end: TimeSpan.Zero)] }
    };

    [Theory]
    [MemberData(nameof(ExclusionCases))]
    public void ExclusionCasesTest(TimeRange a, TimeRange b, TimeRange[] expected) {
        a.Exclude(b).Should().BeEquivalentTo(expected);
    }

    #endregion

    #region Inclusion tests

    public static readonly TheoryData<TimeRange, TimeRange, TimeRange[]> InclusionCases = new() {
        { WorkHours, LunchTime, [WorkHours] },
        { LunchTime, WorkHours, [WorkHours] },
        { WorkHours, Hours24, [Hours24] },
        { LunchTime, Before6, [Before6] },
        { InfiniteDays, Hours24, [InfiniteDays] },
        { Before6, Hours24, [new TimeRange(end: 1.Days())] },
        { LunchTime, new TimeRange(12.Hours() + 30.Minutes(), 14.Hours()), [T(12,14)] },
        { new TimeRange(12.Hours(), 13.Hours() + 30.Minutes()), T(13, 14), [T(12,14)] },
        { LunchTime, T(13, 14), [T(12,14)] }
    };

    [Theory]
    [MemberData(nameof(InclusionCases))]
    public void InclusionCasesTest(TimeRange a, TimeRange b, TimeRange[] expected) {
        a.Include(b).Should().BeEquivalentTo(expected);
    }

    #endregion

    public static readonly TheoryData<string, TimeRange> ParseCases = new() {
        { "[*]", new TimeRange() },
        { "[]", TimeRange.Empty },
        { "[* - 10:00]", new TimeRange(end: 10.Hours()) },
        { "[15:00 - *]", new TimeRange(15.Hours()) },
        { "[10:00 - 15:00]", new TimeRange(10.Hours(), 15.Hours()) },
        { "[* - *]", new TimeRange() }
    };

    [Theory]
    [MemberData(nameof(ParseCases))]
    public void ParseInfiniteDays(string pattern, TimeRange expected) {
        TimeRange.Parse(pattern).Should().Be(expected);
    }
}