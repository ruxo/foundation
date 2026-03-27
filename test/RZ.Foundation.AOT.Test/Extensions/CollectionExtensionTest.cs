namespace RZ.Foundation.Extensions
{
    public sealed class CollectionExtensionTest
    {
        [Test]
        public async ValueTask RemoveAtMiddle() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.RemoveAt(2);
            await Assert.That(result).IsEquivalentTo(new[] {1, 2, 4, 5});
        }

        [Test]
        public async ValueTask RemoveAtInvalidPos_ReturnSame() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.RemoveAt(-2);
            await Assert.That(result).IsEquivalentTo(source);
        }

        [Test]
        public async ValueTask PartitionEvenOddNumbers() {
            var source = new[] {1, 2, 3, 4, 5};
            var (evens, odds) = source.Partition(i => i % 2 == 0);
            await Assert.That(evens).IsEquivalentTo(new[] {2, 4});
            await Assert.That(odds).IsEquivalentTo(new[] {1, 3, 5});
        }

        [Test]
        public async ValueTask TryFirst() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.TryFirst();
            await Assert.That(result.Get()).IsEqualTo(1);
        }

        [Test]
        public async ValueTask TryFirstWithPredicate() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.TryFirst(i => i%2 == 0);
            await Assert.That(result.Get()).IsEqualTo(2);
        }

        [Test]
        public async ValueTask TryFistWithEmpty_ReturnsNone() {
            var source = new int[0];
            var result = source.TryFirst();
            await Assert.That(result.IsNone).IsTrue();
        }

        [Test]
        public async ValueTask TryFirstWithPredicateFalse() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.TryFirst(i => i%7 == 0);
            await Assert.That(result.IsNone).IsTrue();
        }

        [Test]
        public async ValueTask TryFindIndex_ValidCondition() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.TryFindIndex(i => i == 3);
            await Assert.That(result.Get()).IsEqualTo(2);
        }

        [Test]
        public async ValueTask TryFindIndex_NotFoundCondition() {
            var source = new[] {1, 2, 3, 4, 5};
            var result = source.TryFindIndex(i => i == 999);
            await Assert.That(result.IsNone).IsTrue();
        }
    }
}