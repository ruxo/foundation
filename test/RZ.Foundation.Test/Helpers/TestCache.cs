using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt.UnitsOfMeasure;
using Xunit;

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

        public async Task<string> FetcherAsync() {
            ++count;
            await Task.Yield();
            return count.ToString();
        }
    }
    
    [Fact]
    public void TestCacheSync() {
        var mytime = new DateTime(2022, 1, 1);

        using var cache = Cache.Of(new TestData().Fetcher, 1.Minutes(), () => mytime);

        cache.Get().Should().Be("1");
        cache.Get().Should().Be("1"); // second get should use cache

        mytime += 5.Minutes();
        cache.Get().Should().Be("2");
        cache.Get().Should().Be("2");
    }
    
    [Fact]
    public async Task TestCacheAsync() {
        var mytime = new DateTime(2022, 1, 1);

        using var cache = Cache.OfAsync(new TestData().FetcherAsync, 1.Minutes(), () => mytime);

        (await cache.Get()).Should().Be("1");
        (await cache.Get()).Should().Be("1"); // second get should use cache

        mytime += 5.Minutes();
        (await cache.Get()).Should().Be("2");
        (await cache.Get()).Should().Be("2");
    }

    [Fact]
    public void TestMultiaccessSync() {
        var mytime = new DateTime(2022, 1, 1);

        using var cache = Cache.Of(new TestData().Fetcher, 1.Minutes(), () => mytime);

        var result = new ConcurrentQueue<string>();
        void getCache() => result.Enqueue(cache.Get());

        Parallel.Invoke(getCache, getCache, getCache, getCache, getCache, getCache, getCache, getCache);

        result.Should().AllBe("1");
    }

    [Fact]
    public async Task TestMultiaccessAsync() {
        var mytime = new DateTime(2022, 1, 1);

        using var cache = Cache.OfAsync(new TestData().FetcherAsync, 1.Minutes(), () => mytime);

        var result = new ConcurrentQueue<string>();
        async Task getCache() => result.Enqueue(await cache.Get());
        Task<string> getCacheEveryMinute() {
            mytime += 1.Minutes();
            return cache.Get();
        }

        await Task.WhenAll(getCache(), getCache(), getCache(), getCache(), getCache(), getCache(), getCache(), getCache());

        result.Should().AllBe("1");

        var result1 = await Task.WhenAll(getCacheEveryMinute(), getCacheEveryMinute(), getCacheEveryMinute(), getCacheEveryMinute());
        result1.Except(new[]{ "2", "3", "4", "5" }).Should().BeEmpty("But ({0})", string.Join(", ", result1));
    }
}