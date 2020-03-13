using System.Linq;
using FluentAssertions;
using Xunit;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Extensions
{
    public sealed class CollectionExtensionTest
    {
        [Fact]
        public void ChooseSomeOfArray() {
            Option<int>[] data = { None<int>(), 2, 3, None<int>(), 5, 6, None<int>() };

            var result = data.Choose(i => i);

            result.Should().BeEquivalentTo(new[] {2, 3, 5, 6});
        }

        [Fact]
        public void ChooseOdd() {
            var testText = "hello world!";

            var result = testText.Choose((c, i) => i % 2 == 1 ? c : None<char>());

            result.Should().BeEquivalentTo(new[] {'e', 'l', ' ', 'o', 'l', '!'});
        }
    }
}