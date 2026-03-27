using JetBrains.Annotations;
using RZ.Foundation.Extensions;

namespace RZ.Foundation.Test.Extensions;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class StringExtensionTest
{
    [Test]
    public async ValueTask String_iEquals_IgnoresCase() {
        var s = "HeLLo".iEquals("hello");

        await Assert.That(s).IsTrue();
    }

    [Test]
    public async ValueTask String_Limit() {
        var s = "hello".Limit(3);

        await Assert.That(s).IsEqualTo("hel");
    }

    [Test]
    public async ValueTask String_Limit0() {
        var s = "hello".Limit(0);

        await Assert.That(s).IsEqualTo(string.Empty);
    }

    [Test]
    public async ValueTask String_LimitOverflow() {
        var s = "hello".Limit(99);

        await Assert.That(s).IsEqualTo("hello");
    }

    [Test]
    public async ValueTask String_Left() {
        var s = "hello".Left(3);

        await Assert.That(s).IsEqualTo("hel");
    }

    [Test]
    public async ValueTask String_Left0() {
        var s = "hello".Left(0);

        await Assert.That(s).IsEqualTo(string.Empty);
    }

    [Test]
    public async ValueTask String_LeftOverflow() {
        var s = "hello".Left(99);

        await Assert.That(s).IsEqualTo("hello");
    }

    [Test]
    public async ValueTask String_Right() {
        var s = "hello world".Right(3);

        await Assert.That(s).IsEqualTo("rld");
    }

    [Test]
    public async ValueTask String_Right0() {
        var s = "hello world".Right(0);

        await Assert.That(s).IsEqualTo(string.Empty);
    }

    [Test]
    public async ValueTask String_RightOverflow() {
        var s = "hello world".Right(99);

        await Assert.That(s).IsEqualTo("hello world");
    }

    [Test]
    public async ValueTask String_Truncate() {
        var s = "hello".Truncate(4);

        await Assert.That(s).IsEqualTo("hel…");
    }

    [Test]
    public async ValueTask String_TruncateBoundary() {
        var s = "hello".Truncate(5);

        await Assert.That(s).IsEqualTo("hello");
    }

    [Test]
    public async ValueTask String_Truncate0() {
        var s = "hello".Truncate(0);

        await Assert.That(s).IsEqualTo(string.Empty);
    }

    [Test]
    public async ValueTask String_TruncateRight() {
        var s = "hello".TruncateRight(4);

        await Assert.That(s).IsEqualTo("…llo");
    }

    [Test]
    public async ValueTask String_TruncateRightBoundary() {
        var s = "hello".TruncateRight(5);

        await Assert.That(s).IsEqualTo("hello");
    }

    [Test]
    public async ValueTask String_TruncateRight0() {
        var s = "hello".TruncateRight(0);

        await Assert.That(s).IsEqualTo(string.Empty);
    }

    [Test]
    public async ValueTask String_ToDateTime() {
        var s = "2026-03-27 10:11:12".ToDateTime();

        await Assert.That(s.Get()).IsEqualTo(new DateTime(2026, 3, 27, 10, 11, 12));
    }

    [Test]
    public async ValueTask String_ToDateTimeInvalid() {
        var s = "not-a-date".ToDateTime();

        await Assert.That(s.IsNone).IsTrue();
    }

    [Test]
    public async ValueTask String_NotEmpty() {
        var s = "hello".NotEmpty();

        await Assert.That(s).IsEqualTo("hello");
    }

    [Test]
    public async ValueTask String_NotEmptyEmpty() {
        var s = string.Empty.NotEmpty();

        await Assert.That(s).IsNull();
    }

    [Test]
    public async ValueTask String_NotWhiteSpace() {
        var s = "hello".NotWhiteSpace();

        await Assert.That(s).IsEqualTo("hello");
    }

    [Test]
    public async ValueTask String_NotWhiteSpaceWhiteSpace() {
        var s = "   ".NotWhiteSpace();

        await Assert.That(s).IsNull();
    }

    [Test]
    public async ValueTask StringSeq_JoinChar() {
        var s = new[] { "a", "b", "c" }.Join(',');

        await Assert.That(s).IsEqualTo("a,b,c");
    }

    [Test]
    public async ValueTask StringSeq_JoinString() {
        var s = new[] { "a", "b", "c" }.Join(" | ");

        await Assert.That(s).IsEqualTo("a | b | c");
    }
}