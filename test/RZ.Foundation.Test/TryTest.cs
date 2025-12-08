using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace RZ.Foundation;

public class TryTest
{
    [Fact(DisplayName = "Capture error from Task<T>")]
    public async Task TryTaskValue() {
        var (error, result) = await Try(JustThrow());

        error.Should().BeOfType<Exception>();
        result.Should().Be(0);
    }

    [Fact(DisplayName = "Capture result from Task<T>")]
    public async Task TryTaskValueSuccess() {
        var (error, result) = await Try(JustReturn());

        error.Should().BeNull();
        result.Should().Be(123);
    }

    static async ValueTask<int> JustThrow() => throw new Exception("JustThrow");
    static       ValueTask<int> JustReturn() => new(123);
}