using FluentAssertions;
using JetBrains.Annotations;
using LanguageExt;

namespace RZ.Foundation.Extensions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class AsyncEnumerableTest
{
    [Test]
    public async Task MapFromEnumerable() {
        var source = new[]{ 1, 2, 3, 4, 5 };

        async ValueTask<int> mapAsync(int i, int _, CancellationToken __) {
            await Task.Yield();
            return i + 1;
        }

        var result = await source.MapAsync(mapAsync).ToArrayAsync();
        result.Should().BeEquivalentTo([2,3,4,5,6]);
    }
    [Test]
    public async Task ChooseFromEnumerable() {
        var source = new[]{ 1, 2, 3, 4, 5 };

        async ValueTask<Option<int>> chooseAsync(int i, int _, CancellationToken __) {
            await Task.Yield();
            var r = i + 1;
            return r%2 == 0 ? Some(r) : None;
        }

        var result = await source.ChooseAsync(chooseAsync).ToArrayAsync();
        result.Should().BeEquivalentTo([2,4,6]);
    }
    [Test]
    public async Task ChainFromEnumerable() {
        var source = new[]{ 1, 2, 3 };

        IAsyncEnumerable<int> chainAsync(int i, int _) {
            return Enumerable.Repeat(i, i).ToAsyncEnumerable();
        }

        var result = await source.FlattenT(chainAsync).ToArrayAsync();
        result.Should().BeEquivalentTo([1,2,2,3,3,3]);
    }
}