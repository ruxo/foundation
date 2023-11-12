using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Helpers;

public sealed class SnakeCaseTest
{
    [Fact]
    public void OneWord() {
        var result = SnakeCase.ToSnakeCase("Hello");

        result.Should().Be("hello");
    }

    [Fact]
    public void TwoWords() {
        var result = SnakeCase.ToSnakeCase("HelloWorld");

        result.Should().Be("hello_world");
    }

    [Fact]
    public void AbbreviationShouldNotBeSplit() {
        var result = SnakeCase.ToSnakeCase("ABC");

        result.Should().Be("abc");
    }

    [Fact]
    public void TwoSplitWordsShouldJustBeLowerCase() {
        var result = SnakeCase.ToSnakeCase("Hello_World");

        result.Should().Be("hello_world");
    }

    [Fact]
    public void TwoWordsShouldJustBeLowerCase() {
        var result = SnakeCase.ToSnakeCase("Hello World");

        result.Should().Be("hello_world");
    }
}