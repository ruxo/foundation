using JetBrains.Annotations;
using LanguageExt;

namespace RZ.Foundation.Extensions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class AsyncEnumerableTest
{
    [Test]
    public async ValueTask MapFromEnumerable() {
        var source = new[]{ 1, 2, 3, 4, 5 };

        async ValueTask<int> mapAsync(int i, int _, CancellationToken __) {
            await Task.Yield();
            return i + 1;
        }

        var result = await source.MapAsync(mapAsync).ToArrayAsync();
        await Assert.That(result).IsEquivalentTo(new[] {2,3,4,5,6});
    }

    [Test]
    public async ValueTask ChooseFromEnumerable() {
        var source = new[]{ 1, 2, 3, 4, 5 };

        async ValueTask<Option<int>> chooseAsync(int i, int _, CancellationToken __) {
            await Task.Yield();
            var r = i + 1;
            return r%2 == 0 ? Some(r) : None;
        }

        var result = await source.ChooseAsync(chooseAsync).ToArrayAsync();
        await Assert.That(result).IsEquivalentTo(new[] {2,4,6});
    }

    [Test]
    public async ValueTask ChainFromEnumerable() {
        var source = new[]{ 1, 2, 3 };

        IAsyncEnumerable<int> chainAsync(int i, int _) {
            return Enumerable.Repeat(i, i).ToAsyncEnumerable();
        }

        var result = await source.FlattenT(chainAsync).ToArrayAsync();
        await Assert.That(result).IsEquivalentTo(new[] {1,2,2,3,3,3});
    }
}