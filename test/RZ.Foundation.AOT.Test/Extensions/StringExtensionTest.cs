using FluentAssertions;

namespace RZ.Foundation.Extensions;

public sealed class StringExtensionTest
{
    [Test]
    public void String_Left() {
        var s = "hello".Left(3);

        s.Should().Be("hel");
    }

    [Test]
    public void String_Left0() {
        var s = "hello".Left(0);

        s.Should().Be(string.Empty);
    }

    [Test]
    public void String_LeftOverflow() {
        var s = "hello".Left(99);

        s.Should().Be("hello");
    }

    [Test]
    public void String_Right() {
        var s = "hello world".Right(3);

        s.Should().Be("rld");
    }

    [Test]
    public void String_Right0() {
        var s = "hello world".Right(0);

        s.Should().Be(string.Empty);
    }

    [Test]
    public void String_RightOverflow() {
        var s = "hello world".Right(99);

        s.Should().Be("hello world");
    }
}