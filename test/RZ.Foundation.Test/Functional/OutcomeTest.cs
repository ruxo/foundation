using System;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using Xunit;
using static LanguageExt.Prelude;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Functional;

public sealed class OutcomeTest
{
    #region General

    [Fact]
    public void OutcomeDirectSuccessAssignment() {
        var outcome = Success(42).RunIO();

        outcome.IsSuccess.Should().BeTrue();
        outcome.IsFail.Should().BeFalse();
        outcome.Unwrap().Should().Be(42);

        new Action(() => outcome.UnwrapError()).Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void OutcomeDirectFailureAssignment()
    {
        var outcome = Failure<int>(Error.New(123, "dummy")).RunIO();

        outcome.IsSuccess.Should().BeFalse();
        outcome.IsFail.Should().BeTrue();
        outcome.UnwrapError().Should().Match<Error>(e => e.Is(Error.New(123, "another dummy")));

        new Action(() => outcome.Unwrap()).Should().Throw<ExpectedException>();
    }

    #endregion

    #region From other monads

    [Fact]
    public void From_option_some() {
        Option<int> option = 42;

        var result = option.ToOutcome(Error.New(123, "dummy")).RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42);
    }

    [Fact]
    public void From_option_none() {
        Option<int> option = Option<int>.None;

        var result = option.ToOutcome(Error.New(123, "dummy")).RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(Error.New(123, "dummy"));
    }

    #endregion

    #region Monad operations

    [Fact]
    public void Map_value_with_outcome() {
        var outcome = Success(42);

        var result = (from a in outcome
                      select a + 1
                     ).As().RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42 + 1);
    }

    [Fact]
    public void Map_error_with_outcome() {
        var outcome = Failure<int>(Error.New(123, "dummy"));

        var result = outcome.MapFailure(e => Error.New(456, e.Message)).As().RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(Error.New(456, "dummy"));
    }

    [Fact]
    public void Binding_sync_with_sync() {
        var result = from a in Success(42)
                     from b in Success(a + 1)
                     select b;

        result.As().EqualsTo(Success(43)).RunIO().Should().BeTrue();
        result.As().RunIO().Should().Be(SuccessOutcome(43));
    }

    [Fact]
    public async Task Binding_sync_with_async() {
        var result = from a in Success(42)
                     from b in SuccessAsync(a + 1)
                     select b;

        (await result.RunIO()).Should().Be(SuccessOutcome(43));
    }

    #endregion

    #region Catch

    [Fact]
    public void Catch_And_Success() {
        var outcome = Failure<int>(Error.New(123, "dummy"));

        var result = outcome.Catch(_ => 42).As().RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42);
    }

    [Fact]
    public void Catch_And_Failure() {
        var outcome = Failure<int>(Error.New(123, "dummy"));

        var result = outcome.Catch(_ => Error.New(456, "another dummy")).As().RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(Error.New(456, "another dummy"));
    }

    [Fact]
    public void Catch_failure_outcome_with_another_outcome_returns_catch_outcome() {
        var a = Failure<int>(Error.New(42, "dummy"));
        var expected = Failure<int>(Error.New(123, "another dummy"));

        var result = a.Catch(_ => expected).As().RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be(123);
    }

    #endregion

    #region IfFail / IfSuccess

    [Fact]
    public void Get_default_value_from_failure() {
        var outcome = Failure<int>(Error.New(123, "dummy"));

        var result = outcome.IfFail(42).RunIO();

        result.Should().Be(42);
    }

    [Fact]
    public void Get_default_value_by_function_from_failure() {
        var outcome = Failure<int>(Error.New(123, "dummy"));

        var result = outcome.IfFail(e => e.Code).RunIO();

        result.Should().Be(123);
    }

    [Fact]
    public void Perform_action_if_failure() {
        var outcome = Failure<int>(Error.New(123, "dummy"));

        var success = false;
        outcome.IfFail(_ => success = true);

        success.Should().BeTrue();
    }

    [Fact]
    public void Extract_values_and_success_state_from_success_outcome() {
        var outcome = Success(42);

        var success = outcome.IfSuccess(out var v, out _);

        success.Should().BeTrue();
        v.Should().Be(42);
    }

    [Fact]
    public void Extract_values_and_success_state_from_failure_outcome() {
        var outcome = Failure<int>(Error.New(123, "dummy"));

        var success = outcome.IfSuccess(out _, out var e);

        success.Should().BeFalse();
        e.Should().Be(Error.New(123, "dummy"));
    }

    [Fact]
    public void Extract_values_and_failure_state_from_success_outcome() {
        var outcome = Success(42);

        var success = outcome.IfFail(out _, out var v);

        success.Should().BeFalse();
        v.Should().Be(42);
    }

    [Fact]
    public void Extract_values_and_failure_state_from_failure_outcome() {
        var outcome = Failure<int>(Error.New(123, "dummy"));

        var success = outcome.IfFail(out var e, out _);

        success.Should().BeTrue();
        e.Should().Be(Error.New(123, "dummy"));
    }

    #endregion

    #region ToAsync

    [Fact]
    public async Task Convert_success_sync_outcome_to_async_outcome() {
        var outcome = Success(123);
        var expected = SuccessAsync(123);

        var actual = outcome.ToAsync();

        (await actual.EqualsTo(expected).RunIO()).Should().BeTrue();
    }

    [Fact]
    public async Task Convert_failure_sync_outcome_to_async_outcome() {
        var outcome = Failure<int>(Error.New(123, "dummy"));
        var expected = FailureAsync<int>(Error.New(123, "dummy"));

        var actual = outcome.ToAsync();

        (await actual.EqualsTo(expected).RunIO()).Should().BeTrue();
    }

    #endregion

    #region Pipe

    [Fact]
    public void Pipe_two_success_outcomes_returns_first() {
        var a = Success(42);
        var b = Success(123);

        var result = a | b;

        result.EqualsTo(a).RunIO().Should().BeTrue();
    }

    [Fact]
    public void Pipe_two_failure_outcomes_returns_second() {
        var a = Failure<int>(Error.New(42, "dummy"));
        var b = Failure<int>(Error.New(123, "another dummy"));

        var result = a | b;

        result.EqualsTo(b).RunIO().Should().BeTrue();
    }

    [Fact]
    public void Pipe_first_failure_outcome_with_second_success_outcome_returns_second() {
        var a = Failure<int>(Error.New(42, "dummy"));
        var b = Success(123);

        var result = a | b;

        result.EqualsTo(b).RunIO().Should().BeTrue();
    }

    [Fact]
    public void Pipe_first_failure_outcome_with_success_catch_returns_catch_value() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var result = (a | @ifFail(_ => 123)).RunIO();

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(123);
    }

    [Fact]
    public void Pipe_first_failure_outcome_with_failure_catch_returns_catch_value() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var result = (a | @ifFail(_ => Error.New(123, "another dummy"))).RunIO();

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be(123);
    }

    [Fact]
    public void Pipe_failure_outcome_and_perform_side_effect() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var success = false;
        var noChange = true;
        _ = a | failDo(_ => success = true);
        _ = a | @do<int>(_ => noChange = false);

        success.Should().BeTrue();
        noChange.Should().BeTrue();
    }

    [Fact]
    public void Pipe_success_outcome_and_perform_side_effect() {
        var a = Success(42);

        var success = false;
        var noChange = true;
        _ = a | failDo(_ => noChange = false);
        _ = a | @do<int>(_ => success = true);

        success.Should().BeTrue();
        noChange.Should().BeTrue();
    }

    [Fact]
    public void Pipe_failure_outcome_and_catch_for_sideeffect() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var success = false;
        Unit doSomething() {
            success = true;
            return unit;
        }

        _ = a | failDo(_ => doSomething());

        success.Should().BeTrue();
    }

    [Fact]
    public void Pipe_failure_outcome_is_caught_and_replaced_with_value() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var result = (a | @ifFail(Error.New(42, "any text"), 123)).RunIO();

        result.Should().Be(SuccessOutcome(123));
    }

    [Fact]
    public void Pipe_failure_outcome_is_caught_and_replaced_with_another_error() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var result = (a | @ifFail(Error.New(42, "any text"), Error.New(123, "another dummy"))).RunIO();

        result.Should().Be(FailedOutcome<int>(Error.New(123, "another dummy")));
    }

    [Fact]
    public void Pipe_failure_outcome_is_caught_and_replaced_with_value_by_function() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var result = (a | @ifFail(Error.New(42, "any text"), e => e.Code + 1)).RunIO();

        result.Should().Be(SuccessOutcome(43));
    }

    [Fact]
    public void Pipe_failure_outcome_is_caught_and_replaced_with_another_error_by_function() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var result = (a | @ifFail(Error.New(42, "any text"), e => Error.New(e.Code + 1, e.Message))).RunIO();

        result.Should().Be(FailedOutcome<int>(Error.New(43, "dummy")));
    }

    [Fact]
    public void Pipe_success_outcome_and_perform_side_effect_work() {
        var a = Success(42);

        var result = 0;
        _ = a | @do<int>(v => {
                             result = v + 1;
                             return unit;
                         });

        result.Should().Be(43);
    }

    [Fact]
    public async Task Pipe_failure_async_outcome_with_async_failure_outcome() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var result = await (a.ToAsync() | FailureAsync<int>(Error.New(123, "another dummy"))).RunIO();

        result.Should().Be(FailedOutcome<int>(Error.New(123, "another dummy")));
    }

    [Fact]
    public void Perform_side_effect_when_error() {
        var a = Failure<int>(Error.New(42, "dummy"));

        var result = 0;
        _ = a | @ifFail(e => {
                                 result = e.Code + 1;
                             });

        result.Should().Be(43);
    }

    [Fact]
    public void Pipe_unit_outcome_with_iffail_condition_should_not_get_called() {
        var called = false;

        _ = UnitOutcome | @ifFail(_ => {
                                      called = true;
                                  });

        called.Should().BeFalse();
    }

    #endregion
}