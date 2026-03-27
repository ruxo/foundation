namespace RZ.Foundation.Helpers;

public sealed class SnakeCaseTest
{
    [Test]
    public async ValueTask OneWord() {
        var result = SnakeCase.ToSnakeCase("Hello");

        await Assert.That(result).IsEqualTo("hello");
    }

    [Test]
    public async ValueTask TwoWords() {
        var result = SnakeCase.ToSnakeCase("HelloWorld");

        await Assert.That(result).IsEqualTo("hello_world");
    }

    [Test]
    public async ValueTask AbbreviationShouldNotBeSplit() {
        var result = SnakeCase.ToSnakeCase("ABC");

        await Assert.That(result).IsEqualTo("abc");
    }

    [Test]
    public async ValueTask TwoSplitWordsShouldJustBeLowerCase() {
        var result = SnakeCase.ToSnakeCase("Hello_World");

        await Assert.That(result).IsEqualTo("hello_world");
    }

    [Test]
    public async ValueTask TwoWordsShouldJustBeLowerCase() {
        var result = SnakeCase.ToSnakeCase("Hello World");

        await Assert.That(result).IsEqualTo("hello_world");
    }
}