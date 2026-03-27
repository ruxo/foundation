namespace RZ.Foundation.Extensions
{
    public class CollectionExtension_BatchTest
    {
        [Test]
        public async ValueTask BatchWithDividableNumber() {
            var x = Enumerable.Range(1, 100);

            var result = x.Batch(20).ToArray();

            await Assert.That(result.Length).IsEqualTo(5);
            await Assert.That(result[0]).IsEquivalentTo(Enumerable.Range(1, 20));
            await Assert.That(result[1]).IsEquivalentTo(Enumerable.Range(21, 20));
            await Assert.That(result[2]).IsEquivalentTo(Enumerable.Range(41, 20));
            await Assert.That(result[3]).IsEquivalentTo(Enumerable.Range(61, 20));
            await Assert.That(result[4]).IsEquivalentTo(Enumerable.Range(81, 20));
        }

        [Test]
        public async ValueTask BatchWithIndividableNumber() {
            var x = Enumerable.Range(1, 90);

            var result = x.Batch(20).ToArray();

            await Assert.That(result.Length).IsEqualTo(5);
            await Assert.That(result[0]).IsEquivalentTo(Enumerable.Range(1, 20));
            await Assert.That(result[1]).IsEquivalentTo(Enumerable.Range(21, 20));
            await Assert.That(result[2]).IsEquivalentTo(Enumerable.Range(41, 20));
            await Assert.That(result[3]).IsEquivalentTo(Enumerable.Range(61, 20));
            await Assert.That(result[4]).IsEquivalentTo(Enumerable.Range(81, 10));
        }

        [Test]
        public async ValueTask BatchWithEmptySeq() {
            var result = Enumerable.Empty<int>().Batch(20).ToArray();

            await Assert.That(result).IsEmpty();
        }

        [Test]
        public async ValueTask BatchWithGreaterSizeReturnsItself() {
            var x = Enumerable.Range(1, 11).ToArray();

            var result = x.Batch(20).ToArray();

            await Assert.That(result.Length).IsEqualTo(1);
            await Assert.That(result[0]).IsEquivalentTo(x);
        }
    }
}