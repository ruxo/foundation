using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;
// ReSharper disable EqualExpressionComparison

namespace RZ.Foundation
{
    public class OptionTest
    {
        [Fact]
        public void Where_True_NotChangeSuccessToNone() =>
            Assert.Equal(Optional((int?)12).Where(_ => true), Optional((int?)12));

        [Fact]
        public void Where_False_ChangeSuccessToNone() =>
            Assert.Equal(Optional((int?) 12).Where(_ => false), Option<int>.None);

        [Fact]
        public void OptionEquality_SomeEqualsSome() => Optional((int?)12).Equals(Optional((int?)12)).Should().BeTrue();

        [Fact]
        public void OptionEquality_NoneEqualNone() => None.Equals(None).Should().BeTrue();

        [Fact]
        public void OptionEquality_SomeNotEqualsNone() => Optional((int?)0).Equals(None).Should().BeFalse();
    }
}