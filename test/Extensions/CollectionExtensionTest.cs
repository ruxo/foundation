using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Extensions
{
    public sealed class CollectionExtensionTest
    {
        [Fact]
        public void RemoveAtMiddle() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.RemoveAt(2);
            result.Should().BeEquivalentTo(new[] {1, 2, 4, 5});
        }

        [Fact]
        public void RemoveAtInvalidPos_ReturnSame() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.RemoveAt(-2);
            result.Should().BeEquivalentTo(source);
        }

        [Fact]
        public void PartitionEvenOddNumbers() {
            var source = new[] {1, 2, 3, 4, 5};
            var (evens, odds) = source.Partition(i => i % 2 == 0);
            evens.Should().BeEquivalentTo(new[] {2, 4});
            odds.Should().BeEquivalentTo(new[] {1, 3, 5});
        }

        [Fact]
        public void TryFirst() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.TryFirst();
            result.Get().Should().Be(1);
        }

        [Fact]
        public void TryFirstWithPredicate() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.TryFirst(i => i%2 == 0);
            result.Get().Should().Be(2);
        }

        [Fact]
        public void TryFistWithEmpty_ReturnsNone() {
            var source = new int[0];
            var result = source.TryFirst();
            result.IsNone.Should().BeTrue();
        }

        [Fact]
        public void TryFirstWithPredicateFalse() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.TryFirst(i => i%7 == 0);
            result.IsNone.Should().BeTrue();
        }
    }
}