using LanguageExt;
// ReSharper disable EqualExpressionComparison

namespace RZ.Foundation
{
    public class OptionTest
    {
        [Test]
        public async Task Where_True_NotChangeSuccessToNone() =>
            await Assert.That(Optional((int?)12).Where(_ => true)).IsEqualTo(Optional((int?)12));

        [Test]
        public async Task Where_False_ChangeSuccessToNone() =>
            await Assert.That(Optional((int?)12).Where(_ => false)).IsEqualTo(Option<int>.None);

        [Test]
        public async Task OptionEquality_SomeEqualsSome() => await Assert.That(Optional((int?)12).Equals(Optional((int?)12))).IsTrue();

        [Test]
        public async Task OptionEquality_NoneEqualNone() => await Assert.That(None.Equals(None)).IsTrue();

        [Test]
        public async Task OptionEquality_SomeNotEqualsNone() => await Assert.That(Optional((int?)0).Equals(None)).IsFalse();
    }
}
