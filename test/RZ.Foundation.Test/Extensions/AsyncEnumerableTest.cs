using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using LanguageExt;
using Xunit;

namespace RZ.Foundation.Extensions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class AsyncEnumerableTest
{
    [Fact]
    public async Task AverageIntegers()
    {
        var source = new[] {1, 2, 3};

        var result = await source.AsAsyncEnumerable().Average(cancelToken: TestContext.Current.CancellationToken);

        result.Should().Be(2);
    }

    [Fact]
    public async Task MapFromEnumerable() {
        var source = new[]{ 1, 2, 3, 4, 5 };

        async Task<int> mapAsync(int i) {
            await Task.Yield();
            return i + 1;
        }

        var result = await source.MapAsync(mapAsync).ToArrayAsync(TestContext.Current.CancellationToken);
        result.Should().BeEquivalentTo(new[] {2,3,4,5,6});
    }
    [Fact]
    public async Task ChooseFromEnumerable() {
        var source = new[]{ 1, 2, 3, 4, 5 };

        async Task<Option<int>> chooseAsync(int i) {
            await Task.Yield();
            var r = i + 1;
            return r%2 == 0 ? Some(r) : None;
        }

        var result = await source.ChooseAsync(chooseAsync).ToArrayAsync(TestContext.Current.CancellationToken);
        result.Should().BeEquivalentTo(new[] {2,4,6});
    }
    [Fact]
    public async Task ChainFromEnumerable() {
        var source = new[]{ 1, 2, 3 };

        IAsyncEnumerable<int> chainAsync(int i) {
            return Enumerable.Repeat(i, i).AsAsyncEnumerable();
        }

        var result = await source.FlattenT(chainAsync, cancelToken: TestContext.Current.CancellationToken)
                                 .ToArrayAsync(TestContext.Current.CancellationToken);
        result.Should().BeEquivalentTo(new[] {1,2,2,3,3,3});
    }

    [Fact]
    public async Task ContainsHappyPath() {
        var source = new[]{ 1, 2, 3 };

        var result = await source.AsAsyncEnumerable().Contains(2, TestContext.Current.CancellationToken);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Skip()
    {
        var source = new[] {1, 2, 3};
        var result = await source.AsAsyncEnumerable()
                                 .Skip(2, cancelToken: TestContext.Current.CancellationToken)
                                 .ToArrayAsync(TestContext.Current.CancellationToken);
        result.Should().BeEquivalentTo(new[] {3});
    }

    [Fact]
    public async Task SkipWhile()
    {
        var source = new[] {1, 2, 3};
        var result = await source.AsAsyncEnumerable()
                                 .SkipWhile(x => x < 3, cancelToken: TestContext.Current.CancellationToken)
                                 .ToArrayAsync(TestContext.Current.CancellationToken);
        result.Should().BeEquivalentTo(new[] {3});
    }

    [Fact]
    public async Task Take()
    {
        var source = new[] {1, 2, 3};
        var result = await source.AsAsyncEnumerable().Take(2, cancelToken: TestContext.Current.CancellationToken)
                                 .ToArrayAsync(TestContext.Current.CancellationToken);
        result.Should().BeEquivalentTo(new[] {1,2});
    }
}