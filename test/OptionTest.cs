using FluentAssertions;
using Xunit;
using static RZ.Foundation.Prelude;

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

    }
}