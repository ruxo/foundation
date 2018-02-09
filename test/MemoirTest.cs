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
    }
}