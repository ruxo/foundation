using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using RZ.Foundation.Types;
using Xunit;
using static RZ.Foundation.Prelude;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Functional;

public sealed class OutcomeAsyncTest
{
    #region General

    [Fact]
    public async Task Direct_assign_to_OutcomeAsync()
    {
        var outcomeAsync = SuccessAsync(42);

        var outcome = await outcomeAsync.RunIO();

        outcome.IsSuccess.Should().BeTrue();
        outcome.IsFail.Should().BeFalse();
    }

    [Fact]
    public async Task OutcomeAsyncDirectFailureAssignment()
    {
        var outcomeAsync = FailureAsync<int>(new ErrorInfo("dummy", "dummy"));

        var outcome = await outcomeAsync.RunIO();

        outcome.IsSuccess.Should().BeFalse();
        outcome.IsFail.Should().BeTrue();

        outcome.UnwrapError().Should().Match<ErrorInfo>(e => e.Is("dummy"));
    }

    #endregion

    #region Monad operations

    [Fact]
    public async Task Map_value_with_outcome_async() {
        var outcomeAsync = SuccessAsync(42);

        var result = await outcomeAsync.Map(x => x + 1).As().RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42 + 1);
    }

    [Fact]
    public async Task Add_two_outcomes_async() {
        var a = SuccessAsync(42);
        var b = SuccessAsync(123);

        var result = await (from x in a
                            from y in b
                            let r = x + y
                            select r).As().RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42 + 123);
    }

    [Fact]
    public async Task Binding_async_with_sync_outcome() {
        var result = from a in SuccessAsync(42)
                     from b in SuccessAsync(a + 1)
                     select b;

        (await result.As().RunIO()).Should().Be(SuccessOutcome(43));
    }

    [Fact]
    public async Task Binding_with_sync_outcome() {
        var result = from a in SuccessAsync(42)
                     from b in Success(a + 1)
                     select b;

        (await result.As().RunIO()).Should().Be(SuccessOutcome(43));
    }

    #endregion

    #region Catch

    [Fact]
    public async Task Catch_And_Success() {
        var outcomeAsync = FailureAsync<int>(new ErrorInfo("123", "dummy"));

        var result = await outcomeAsync.Catch(_ => 42).As().RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42);
    }

    [Fact]
    public async Task Catch_And_Failure() {
        var outcomeAsync = FailureAsync<int>(new ErrorInfo("123", "dummy"));

        var result = await outcomeAsync.Catch(_ => new ErrorInfo("456", "another dummy")).As().RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(new ErrorInfo("456", "another dummy"));
    }

    #endregion

    #region IfFail / IfSuccess

    [Fact]
    public async Task Get_default_value_from_failure() {
        var outcomeAsync = FailureAsync<int>(new ErrorInfo("123", "dummy"));

        var result = await outcomeAsync.IfFail(42).RunIO();

        result.Should().Be(42);
    }

    [Fact]
    public async Task Get_default_value_by_function_from_failure() {
        var outcomeAsync = FailureAsync<int>(new ErrorInfo("123", "dummy"));

        var result = await outcomeAsync.IfFail(e => int.Parse(e.Code)).RunIO();

        result.Should().Be(123);
    }

    [Fact]
    public async Task Perform_action_if_failure() {
        var outcomeAsync = FailureAsync<int>(new ErrorInfo("123", "dummy"));

        var success = false;
        await outcomeAsync.IfFail(_ => success = true).RunIO();

        success.Should().BeTrue();
    }

    #endregion

    #region Pipe

    [Fact]
    public async Task Pipe_two_success_outcomes_returns_first() {
        var a = SuccessAsync(42);
        var b = SuccessAsync(123);

        var result = await (a | b).RunIO();

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public async Task Pipe_two_failure_outcomes_returns_second() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));
        var b = FailureAsync<int>(new ErrorInfo("123", "another dummy"));

        var result = await (a | b).RunIO();

        result.Should().Be(FailedOutcome<int>(new ErrorInfo("123", "another dummy")));
    }

    [Fact]
    public async Task Pipe_first_failure_outcome_with_second_success_outcome_returns_second() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));
        var b = SuccessAsync(123);

        var result = await (a | b).RunIO();

        result.Should().Be(SuccessOutcome(123));
    }

    [Fact]
    public async Task Pipe_first_failure_outcome_with_success_catch_returns_catch_value() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));

        var result = await (a | @catch(_ => 123)).RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(123);
    }

    [Fact]
    public async Task Pipe_first_failure_outcome_with_failure_catch_returns_catch_value() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));

        var result = await (a | @catch<int>(_ => new ErrorInfo("123", "another dummy"))).RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be("123");
    }

    [Fact]
    public async Task Pipe_failure_outcome_and_perform_side_effect() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));

        var success = false;
        var noChange = true;
        _ = await (a | failDo(_ => success = true)).RunIO();
        _ = await (a | @do<int>(_ => noChange = false)).RunIO();

        success.Should().BeTrue();
        noChange.Should().BeTrue();
    }

    [Fact]
    public async Task Pipe_success_outcome_and_perform_side_effect() {
        var a = SuccessAsync(42);

        var success = false;
        var noChange = true;
        _ = await (a | failDo(_ => noChange = false)).RunIO();
        _ = await (a | @do<int>(_ => success = true)).RunIO();

        success.Should().BeTrue();
        noChange.Should().BeTrue();
    }

    [Fact]
    public async Task Pipe_failure_outcome_and_catch_with_another_outcome_returns_catch_outcome() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));
        var expected = FailureAsync<int>(new ErrorInfo("123", "another dummy"));

        var result = await (a | expected).RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be("123");
    }

    [Fact]
    public async Task Pipe_failure_outcome_async_and_a_success_outcome_returns_the_success_one() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));
        var b = Success(123);

        var result = await (a | b.ToAsync()).RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(123);
    }

    [Fact]
    public async Task Pipe_failure_outcome_async_and_a_success_outcome_catch_returns_the_success_outcome() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));
        Outcome<int> b = 123;

        var result = await (a | @catch(_ => b)).RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(123);
    }

    [Fact]
    public async Task Pipe_failure_outcome_async_and_async_failure_outcome_catch_returns_the_failure_outcome() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));
        var b = FailureAsync<int>(new ErrorInfo("123", "another dummy"));

        var result = await a.Catch(_ => b).As().RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be("123");
    }

    [Fact]
    public async Task Pipe_failure_outcome_async_and_catch_for_sideeffect() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));

        var success = false;
        async Task<Unit> doSomething() {
            await Task.Yield();
            success = true;
            return unit;
        }

        _ = await a.IfFail(_ => doSomething()).RunIO();

        success.Should().BeTrue();
    }

    [Fact]
    public async Task Perform_side_effect_when_error() {
        var a = FailureAsync<int>(new ErrorInfo("42", "dummy"));

        var result = 0;
        _ = await (a | @failDo(e => result = int.Parse(e.Code) + 1)).RunIO();

        result.Should().Be(43);
    }

    [Fact]
    public async Task Pipe_unit_outcome_with_iffail_condition_should_not_get_called() {
        var called = false;

        _ = await (UnitOutcomeAsync | @failDo(_ => called = true)).RunIO();

        called.Should().BeFalse();
    }

    #endregion
}