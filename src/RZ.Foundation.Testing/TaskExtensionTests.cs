using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Testing;

public sealed class TaskExtensionTests
{
    [Fact(DisplayName = "Task not error, OnError just returns")]
    public async Task OnError_WithSuccess() {
        var x = Task.FromResult(123);

        var z = Try(x).Catch(_ => -1);

        var y = await new OnHandler<int>(x).Catch(_ => -1);

        y.Should().Be(123);
    }

    [Fact(DisplayName = "Task error, OnError returns -1")]
    public async Task OnError_WithException() {
        var x = Task.FromException<int>(new Exception("Test"));

        var y = await new OnHandler<int>(x).Catch(_ => -1);

        y.Should().Be(-1);
    }

    [Fact(DisplayName = "Task error, do an effect")]
    public async Task OnError_EffectBeforeThrow() {
        var x = Task.FromException<int>(new Exception("Test"));

        bool effect = false;

        Func<Task> action = () => Try(x).BeforeThrow(_ => effect = true);

        await action.Should().ThrowAsync<Exception>();
        effect.Should().BeTrue();
    }
}