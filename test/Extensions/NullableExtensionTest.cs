using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Extensions
{
    public class NullableExtensionTest
    {
        [Fact]
        public void RefTryToValue() =>
            "123".Try(int.Parse).Should().Be(123);

        [Fact]
        public void ValueTryToRef() =>
            ((int?) 123).Try(i => i.ToString()).Should().Be("123");

        [Fact]
        public void RefTryToRef() =>
            "123".Try(s => s.Select(c => (int) c).ToArray()).Should().BeEquivalentTo(new[] {0x31, 0x32, 0x33});

        [Fact]
        public void ValueTryToValue() =>
            ((int?) 2020).Try(y => new DateTime(y, 1, 1)).Should().Be(new DateTime(2020, 1, 1));
    }
}