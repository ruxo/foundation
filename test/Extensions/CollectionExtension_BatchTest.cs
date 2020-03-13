using System.Linq;
using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Extensions
{
    public class CollectionExtension_BatchTest
    {
        [Fact]
        public void BatchWithDividableNumber() {
            var x = Enumerable.Range(1, 100);

            var result = x.Batch(20).ToArray();

            result.Length.Should().Be(5);
            result[0].Should().Equal(Enumerable.Range(1, 20));
            result[1].Should().Equal(Enumerable.Range(21, 20));
            result[2].Should().Equal(Enumerable.Range(41, 20));
            result[3].Should().Equal(Enumerable.Range(61, 20));
            result[4].Should().Equal(Enumerable.Range(81, 20));
        }
        [Fact]
        public void BatchWithIndividableNumber() {
            var x = Enumerable.Range(1, 90);

            var result = x.Batch(20).ToArray();

            result.Length.Should().Be(5);
            result[0].Should().Equal(Enumerable.Range(1, 20));
            result[1].Should().Equal(Enumerable.Range(21, 20));
            result[2].Should().Equal(Enumerable.Range(41, 20));
            result[3].Should().Equal(Enumerable.Range(61, 20));
            result[4].Should().Equal(Enumerable.Range(81, 10));
        }

        [Fact]
        public void BatchWithEmptySeq() {
            var result = Enumerable.Empty<int>().Batch(20).ToArray();

            result.Should().BeEmpty();
        }

        [Fact]
        public void BatchWithGreaterSizeReturnsItself() {
            var x = Enumerable.Range(1, 11).ToArray();

            var result = x.Batch(20).ToArray();

            result.Length.Should().Be(1);
            result[0].Should().BeEquivalentTo(x);
        }
    }
}
