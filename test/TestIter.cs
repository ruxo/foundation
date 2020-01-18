using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace RZ.Foundation
{
    public class TestIter
    {
        readonly ITestOutputHelper output;
        public TestIter(ITestOutputHelper output) {
            this.output = output;
        }

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

        [Fact]
        public void MultithreadTest() {
            var x = Prelude.IterSafe(Enumerable.Range(1, 1_000_000));

            var startSignal = new ManualResetEvent(false);

            Task<int> getLast(int n) => Task.Run(() => {
                startSignal.WaitOne();
                return x.Take(n).Last();
            });

            var tasks = new[] {getLast(100_000), getLast(100_100), getLast(100_200), getLast(110_000), Task.Run(() => x.Last())};

            startSignal.Set();

            try {
                Task.WaitAll(tasks.Select(t => (Task)t).ToArray());
            }
            catch (Exception e) {
                output.WriteLine(e.ToString());
            }

            var result = from t in tasks select t.Result;
            result.Should().BeEquivalentTo(new[] {100_000, 100_100, 100_200, 110_000, 1_000_000 });
        }
    }
}