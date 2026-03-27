using System.Collections.Concurrent;
using LanguageExt.UnitsOfMeasure;

namespace RZ.Foundation.Helpers;

public sealed class TestCache
{
    sealed class TestData
    {
        int count;

        public string Fetcher() {
            ++count;
            return count.ToString();
        }

        public async ValueTask<string> FetcherAsync() {
            ++count;
            await Task.Yield();
            return count.ToString();
        }
    }

    [Test]
    public async ValueTask TestCacheSync() {
        var mytime = new DateTime(2022, 1, 1);

        using var cache = Cache.Of(new TestData().Fetcher, 1.Minutes(), () => mytime);

        await Assert.That(cache.Get()).IsEqualTo("1");
        await Assert.That(cache.Get()).IsEqualTo("1"); // second get should use cache

        mytime += 5.Minutes();
        await Assert.That(cache.Get()).IsEqualTo("2");
        await Assert.That(cache.Get()).IsEqualTo("2");
    }

    [Test]
    public async ValueTask TestCacheAsync() {
        var mytime = new DateTime(2022, 1, 1);

        using var cache = Cache.OfAsync(new TestData().FetcherAsync, 1.Minutes(), () => mytime);

        await Assert.That(await cache.Get()).IsEqualTo("1");
        await Assert.That(await cache.Get()).IsEqualTo("1"); // second get should use cache

        mytime += 5.Minutes();
        await Assert.That(await cache.Get()).IsEqualTo("2");
        await Assert.That(await cache.Get()).IsEqualTo("2");
    }

    [Test]
    public async ValueTask TestMultiaccessSync() {
        var mytime = new DateTime(2022, 1, 1);

        using var cache = Cache.Of(new TestData().Fetcher, 1.Minutes(), () => mytime);

        var result = new ConcurrentQueue<string>();
        void getCache() => result.Enqueue(cache.Get());

        Parallel.Invoke(getCache, getCache, getCache, getCache, getCache, getCache, getCache, getCache);

        await Assert.That(result.All(r => r == "1")).IsTrue();
    }

    [Test]
    public async Task TestMultiaccessAsync() {
        var mytime = new DateTime(2022, 1, 1);

        using var cache = Cache.OfAsync(new TestData().FetcherAsync, 1.Minutes(), () => mytime);

        var result = new ConcurrentQueue<string>();
        async Task getCache() => result.Enqueue(await cache.Get());

        await Task.WhenAll(getCache(), getCache(), getCache(), getCache(), getCache(), getCache(), getCache(), getCache());

        await Assert.That(result.All(r => r == "1")).IsTrue();

        var result1 = await Task.WhenAll(getCacheEveryMinute(), getCacheEveryMinute(), getCacheEveryMinute(), getCacheEveryMinute());
        await Assert.That(result1.Except(new[] { "2", "3", "4", "5" })).IsEmpty();
        return;

        async Task<string> getCacheEveryMinute() {
            mytime += 1.Minutes();
            return await cache.Get();
        }
    }
}