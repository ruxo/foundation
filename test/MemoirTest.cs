using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RZ.Foundation.Helpers;
using Xunit;

namespace RZ.Foundation
{
    public class MemoirTest
    {
        [Fact]
        public void MemoirDictSameKey() {
            var sideeffect = 0;
            int test(int x) {
                ++sideeffect;
                return x + sideeffect;
            }
            var memoir = Memoir.DictWith<int,int>(test);
            memoir(1);

            var result = memoir(1);

            result.Should().NotBe(3, "sideeffect should be increased once only!");
            result.Should().Be(2);
        }

        [Fact]
        public async Task MemoirDictSameKeyMultithread() {
            var sideeffect = 0;
            int test(int x) {
                ++sideeffect;
                return x + sideeffect;
            }
            var memoir = Memoir.From(test, new Memoir.DictionaryCache<int,int>(), new Memoir.MultithreadLocker<int>());
            var startLine = new ManualResetEventSlim();
            var tasks = Enumerable.Range(1, 1000).Select(_ => Task.Run(() => {
                startLine.Wait();
                return memoir(1);
            })).ToArray();

            startLine.Set();    // go!

            await Task.WhenAll(tasks.Cast<Task>().ToArray());

#pragma warning disable xUnit1031
            var results = tasks.Select(t => t.Result).ToArray();
#pragma warning restore xUnit1031

            results.All(r => r == 2).Should().BeTrue();
            sideeffect.Should().Be(1, "sideeffect should be called just once!");
        }
    }
}