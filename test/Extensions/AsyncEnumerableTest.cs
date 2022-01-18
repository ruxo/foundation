using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Extensions
{
    public class AsyncEnumerableTest
    {
        [Fact]
        public async Task MapFromEnumerable() {
            var source = new[]{ 1, 2, 3, 4, 5 };

            async Task<int> mapAsync(int i) {
                await Task.Yield();
                return i + 1;
            }

            var result = await source.MapAsync(mapAsync).ToArrayAsync();
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

            var result = await source.ChooseAsync(chooseAsync).ToArrayAsync();
            result.Should().BeEquivalentTo(new[] {2,4,6});
        }
        [Fact]
        public async Task ChainFromEnumerable() {
            var source = new[]{ 1, 2, 3 };

            IAsyncEnumerable<int> chainAsync(int i) {
                return Enumerable.Repeat(i, i).AsAsyncEnumerable();
            }

            var result = await source.FlattenT(chainAsync).ToArrayAsync();
            result.Should().BeEquivalentTo(new[] {1,2,2,3,3,3});
        }
    }
}