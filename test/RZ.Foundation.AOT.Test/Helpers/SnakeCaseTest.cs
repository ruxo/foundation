using FluentAssertions;

namespace RZ.Foundation.Helpers;

public sealed class SnakeCaseTest
{
    [Test]
    public void OneWord() {
        var result = SnakeCase.ToSnakeCase("Hello");

        result.Should().Be("hello");
    }

    [Test]
    public void TwoWords() {
        var result = SnakeCase.ToSnakeCase("HelloWorld");

        result.Should().Be("hello_world");
    }

    [Test]
    public void AbbreviationShouldNotBeSplit() {
        var result = SnakeCase.ToSnakeCase("ABC");

        result.Should().Be("abc");
    }

    [Test]
    public void TwoSplitWordsShouldJustBeLowerCase() {
        var result = SnakeCase.ToSnakeCase("Hello_World");

        result.Should().Be("hello_world");
    }

    [Test]
    public void TwoWordsShouldJustBeLowerCase() {
        var result = SnakeCase.ToSnakeCase("Hello World");

        result.Should().Be("hello_world");
    }
}