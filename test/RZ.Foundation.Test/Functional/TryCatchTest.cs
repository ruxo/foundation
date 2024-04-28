using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static LanguageExt.Prelude;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Functional;

public sealed class TryCatchTest
{
    [Fact]
    public void Try_catch_sync_outcome() {
        var outcome = Success(42);

        var result = TryCatch(() => outcome).RunIO();

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public async Task Try_catch_async_outcome() {
        var outcome = Task.FromResult(SuccessOutcome(42));

        var result = await TryCatch(() => outcome).RunIO();

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public void Try_catch_value() {
        var result = TryCatch(() => 42).RunIO();

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public async Task Try_catch_async_value() {
        var result = await TryCatch(() => Task.FromResult(42)).RunIO();

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public void Try_catch_action() {
        var called = false;
        var result = TryCatch(() => {
                                  called = true;
                              }).RunIO();

        result.Should().Be(SuccessOutcome(unit));
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Try_catch_async_action() {
        var called = false;
        var result = await TryCatch(async () => {
            await Task.Yield();
            called = true;
        }).RunIO();

        result.Should().Be(SuccessOutcome(unit));
        called.Should().BeTrue();
    }

    [Fact]
    public void Try_catch_action_exception() {
        void test() {
            throw new Exception("test");
        }
        var result = TryCatch(test).RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().IsExceptional.Should().BeTrue();
    }

    [Fact]
    public async Task Try_catch_async_action_exception() {
        async Task test() {
            await Task.Yield();
            throw new Exception("test");
        }
        var result = await TryCatch(test).RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().IsExceptional.Should().BeTrue();
    }
}