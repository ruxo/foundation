using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RZ.Foundation.Types;
using Xunit;

namespace RZ.Foundation
{
    public class TestIter
    {
        [Fact]
        public void NormalScenarioWithArray() {
            var x = Prelude.Iter(new[] {1, 2, 3, 4, 5});

            var a = x.ToArray();
            var b = x.ToArray();

            a.Length.Should().Be(5);
            a.Should().BeEquivalentTo(new[] {1, 2, 3, 4, 5});

            b.Length.Should().Be(5);
            b.Should().BeEquivalentTo(a);
        }

        [Fact]
        public void IterWithSingleMember() {
            Iter<int> x = 3;

            x.Count().Should().Be(1);
            x.ToArray()[0].Should().Be(3);
        }

        [Fact]
        public void NoSecondIterationForCacheEnabled() {
            var iterationCount = 0;

            IEnumerable<int> generator() {
                ++iterationCount;
                for (var i = 1; i <= 5; ++i)
                    yield return i;
            }

            var x = Prelude.Iter(generator());
            var y = Prelude.Iter(x);

            y.Should().BeSameAs(x);
            x.ToArray().Should().BeEquivalentTo(new[] {1, 2, 3, 4, 5});
            x.ToArray().Should().BeEquivalentTo(y.ToArray());
            iterationCount.Should().Be(1, "generator should be called once!");
        }

        [Fact]
        public void FetchCacheWithDifferentLength() {
            var x = Prelude.Iter(Enumerable.Range(1, 10));

            var shortVersion = x.Take(3).ToArray();
            var longVersion = x.Take(5).ToArray();

            shortVersion.Should().BeEquivalentTo(new[] {1, 2, 3});
            longVersion.Should().BeEquivalentTo(new[] {1, 2, 3, 4, 5});
        }
    }
}