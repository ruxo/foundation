using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Extensions;

public sealed class StringExtensionTest
{
    [Fact]
    public void String_Left() {
        var s = "hello".Left(3);

        s.Should().Be("hel");
    }
    
    [Fact]
    public void String_Left0() {
        var s = "hello".Left(0);

        s.Should().Be(string.Empty);
    }
    
    [Fact]
    public void String_LeftOverflow() {
        var s = "hello".Left(99);

        s.Should().Be("hello");
    }
    
    [Fact]
    public void String_Right() {
        var s = "hello world".Right(3);

        s.Should().Be("rld");
    }
    
    [Fact]
    public void String_Right0() {
        var s = "hello world".Right(0);

        s.Should().Be(string.Empty);
    }
    
    [Fact]
    public void String_RightOverflow() {
        var s = "hello world".Right(99);

        s.Should().Be("hello world");
    }
}