using System;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt.Common;
using Xunit;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Functional;

public sealed class OutcomeAsyncTest
{
    #region General

    [Fact]
    public async Task Direct_assign_to_OutcomeAsync()
    {
        OutcomeAsync<int> outcomeAsync = 42;

        var outcome = await outcomeAsync;

        outcome.IsSuccess.Should().BeTrue();
        outcome.IsFail.Should().BeFalse();
        outcome.Unwrap().Should().Be(42);

        new Action(() => outcome.UnwrapError()).Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public async Task OutcomeAsyncDirectFailureAssignment()
    {
        OutcomeAsync<int> outcomeAsync = Error.New(123, "dummy");

        var outcome = await outcomeAsync;

        outcome.IsSuccess.Should().BeFalse();
        outcome.IsFail.Should().BeTrue();
        outcome.UnwrapError().Should().Match<Error>(e => e.Is(Error.New(123, "another dummy")));

        new Action(() => outcome.Unwrap()).Should().Throw<ExpectedException>();
    }

    #endregion

    #region Catch

    [Fact]
    public async Task Catch_And_Success() {
        OutcomeAsync<int> outcomeAsync = Error.New(123, "dummy");

        var result = await outcomeAsync.Catch(_ => 42);

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42);
    }

    [Fact]
    public async Task Catch_And_Failure() {
        OutcomeAsync<int> outcomeAsync = Error.New(123, "dummy");

        var result = await outcomeAsync.Catch(_ => Error.New(456, "another dummy"));

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(Error.New(456, "another dummy"));
    }

    #endregion

    #region IfFail / IfSuccess

    [Fact]
    public async Task Get_default_value_from_failure() {
        OutcomeAsync<int> outcomeAsync = Error.New(123, "dummy");

        var result = await outcomeAsync.IfFail(42);

        result.Should().Be(42);
    }

    [Fact]
    public async Task Get_default_value_by_function_from_failure() {
        OutcomeAsync<int> outcomeAsync = Error.New(123, "dummy");

        var result = await outcomeAsync.IfFail(e => e.Code);

        result.Should().Be(123);
    }

    [Fact]
    public async Task Perform_action_if_failure() {
        OutcomeAsync<int> outcomeAsync = Error.New(123, "dummy");

        var success = false;
        await outcomeAsync.IfFail(_ => success = true);

        success.Should().BeTrue();
    }

    #endregion

    #region ToAsync

    [Fact]
    public async Task Convert_outcome_async_to_outcome_task() {
        OutcomeAsync<int> outcomeAsync = 123;
        Outcome<int> expected = 123;

        var actual = await outcomeAsync.AsTask();

        actual.Should().Be(expected);
    }

    #endregion

    #region Pipe

    [Fact]
    public async Task Pipe_two_success_outcomes_returns_first() {
        OutcomeAsync<int> a = 42;
        OutcomeAsync<int> b = 123;

        var result = await (a | b);

        result.Should().Be(SuccessOutcome(42));
    }

    [Fact]
    public async Task Pipe_two_failure_outcomes_returns_second() {
        OutcomeAsync<int> a = Error.New(42, "dummy");
        OutcomeAsync<int> b = Error.New(123, "another dummy");

        var result = await (a | b);

        result.Should().Be(FailedOutcome<int>(Error.New(123, "another dummy")));
    }

    [Fact]
    public async Task Pipe_first_failure_outcome_with_second_success_outcome_returns_second() {
        OutcomeAsync<int> a = Error.New(42, "dummy");
        OutcomeAsync<int> b = 123;

        var result = await (a | b);

        result.Should().Be(SuccessOutcome(123));
    }

    [Fact]
    public async Task Pipe_first_failure_outcome_with_success_catch_returns_catch_value() {
        OutcomeAsync<int> a = Error.New(42, "dummy");

        var result = await (a | @ifFail(_ => 123));

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(123);
    }

    [Fact]
    public async Task Pipe_first_failure_outcome_with_failure_catch_returns_catch_value() {
        OutcomeAsync<int> a = Error.New(42, "dummy");

        var result = await (a | @ifFail(_ => Error.New(123, "another dummy")));

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be(123);
    }

    [Fact]
    public async Task Pipe_failure_outcome_and_perform_side_effect() {
        OutcomeAsync<int> a = Error.New(42, "dummy");

        var success = false;
        var noChange = true;
        _ = await (a | failDo(_ => success = true));
        _ = await (a | @do<int>(_ => noChange = false));

        success.Should().BeTrue();
        noChange.Should().BeTrue();
    }

    [Fact]
    public async Task Pipe_success_outcome_and_perform_side_effect() {
        OutcomeAsync<int> a = 42;

        var success = false;
        var noChange = true;
        _ = await (a | failDo(_ => noChange = false));
        _ = await (a | @do<int>(_ => success = true));

        success.Should().BeTrue();
        noChange.Should().BeTrue();
    }

    [Fact]
    public async Task Pipe_failure_outcome_and_catch_with_another_outcome_returns_catch_outcome() {
        OutcomeAsync<int> a = Error.New(42, "dummy");
        OutcomeAsync<int> expected = Error.New(123, "another dummy");

        var result = await (a | @ifFail(expected));

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be(123);
    }

    #endregion
}