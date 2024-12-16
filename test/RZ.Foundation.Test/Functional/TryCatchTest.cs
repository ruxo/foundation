using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Functional;

public sealed class TryCatchTest
{
    [Fact]
    public void Try_catch_sync_outcome() {
        var outcome = SuccessOutcome(42);

        var result = TryCatch(() => outcome);

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public async Task Try_catch_async_outcome() {
        var outcome = Task.FromResult(SuccessOutcome(42));

        var result = await TryCatch(() => outcome);

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public void Try_catch_value() {
        var result = TryCatch(() => 42);

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public async Task Try_catch_async_value() {
        var result = await TryCatch(() => Task.FromResult(42));

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public void Try_catch_action() {
        var called = false;
        var result = TryCatch(() => {
                                  called = true;
                              });

        result.Should().Be(SuccessOutcome(unit));
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Try_catch_async_action() {
        var called = false;
        var result = await TryCatch(async () => {
            await Task.Yield();
            called = true;
        });

        result.Should().Be(SuccessOutcome(unit));
        called.Should().BeTrue();
    }

    [Fact]
    public void Try_catch_action_exception() {
        var result = TryCatch(Test);

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Is(StandardErrorCodes.Unhandled).Should().BeTrue();
        result.UnwrapError().Message.Should().Be("test");
        return;

        void Test() {
            throw new Exception("test");
        }
    }

    [Fact]
    public async Task Try_catch_async_action_exception() {
        var result = await TryCatch(Test);

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Is(StandardErrorCodes.Unhandled).Should().BeTrue();
        result.UnwrapError().Message.Should().Be("test");
        return;

        async Task Test() {
            await Task.Yield();
            throw new Exception("test");
        }
    }
}