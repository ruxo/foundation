using FluentAssertions;
using Xunit;
using static RZ.Foundation.Prelude;
// ReSharper disable EqualExpressionComparison

namespace RZ.Foundation
{
    public class OptionTest
    {
        [Fact]
        public void OrElseWithPlainValue_Over_SomeNumber() => 12.ToOption().OrElse(13).Should().Be(12.ToOption());
        [Fact]
        public void OrElseWithPlainValue_Over_None() => Option<int>.None().OrElse(13).Should().Be(13.ToOption());

        [Fact]
        public void Where_True_NotChangeSuccessToNone() => Optional(12).Where(_ => true).Should().Be(Optional(12));
        [Fact]
        public void Where_False_ChangeSuccessToNone() => Optional(12).Where(_ => false).Should().Be(None<int>());

        [Fact]
        public void OptionEquality_SomeEqualsSome() => Optional(12).Equals(Optional(12)).Should().BeTrue();

        [Fact]
        public void OptionEquality_NoneEqualNone() => None<int>().Equals(None<int>()).Should().BeTrue();

        [Fact]
        public void OptionEquality_SomeNotEqualsNone() => Optional(0).Equals(None<int>()).Should().BeFalse();
    }
}