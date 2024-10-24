using System;
using System.Text.Json;
using FluentAssertions;
using LanguageExt;
using LanguageExt.Common;
using RZ.Foundation.Extensions;
using RZ.Foundation.Json;
using RZ.Foundation.Types;
using Xunit;
using static LanguageExt.Prelude;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Functional;

public sealed class OutcomeTest
{
    #region General

    [Fact]
    public void OutcomeDirectSuccessAssignment() {
        var outcome = SuccessOutcome(42);

        outcome.IsSuccess.Should().BeTrue();
        outcome.IsFail.Should().BeFalse();
        outcome.Unwrap().Should().Be(42);

        new Action(() => outcome.UnwrapError()).Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void OutcomeDirectFailureAssignment()
    {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        outcome.IsSuccess.Should().BeFalse();
        outcome.IsFail.Should().BeTrue();
        outcome.UnwrapError().Should().Match<ErrorInfo>(e => e.Is("123"));

        new Action(() => outcome.Unwrap()).Should().Throw<ErrorInfoException>();
    }

    [Fact]
    public void Outcome_success_equality() {
        var a = SuccessOutcome(42);
        var b = SuccessOutcome(42);

        var result = a.Equals(b);

        result.Should().BeTrue();
    }

    [Fact]
    public void Convert_from_error() {
        var err = Error.New(123, "dummy");

        Outcome<string> result = err;

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be(StandardErrorCodes.Unhandled);
    }

    [Fact]
    public void Convert_from_either_error() {
        Either<ErrorInfo, string> err = new ErrorInfo(StandardErrorCodes.Timeout, "dummy");

        Outcome<string> result = err;

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be(StandardErrorCodes.Timeout);
    }

    [Fact]
    public void Convert_from_either_success() {
        Either<ErrorInfo, string> err = "dummy";

        Outcome<string> result = err;

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be("dummy");
    }

    [Fact]
    public void Default_value_for_failure() {
        var value = FailedOutcome<int>(new ErrorInfo(StandardErrorCodes.Unhandled, "dummy"));

        var result = value.IfFail(123);

        result.Should().Be(123);
    }

    #endregion

    #region From other monads

    [Fact]
    public void From_option_some() {
        Option<int> option = 42;

        var result = option.ToOutcome(new ErrorInfo("123", "dummy"));

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42);
    }

    [Fact]
    public void From_option_none() {
        Option<int> option = Option<int>.None;

        var result = option.ToOutcome(new ErrorInfo("123", "dummy"));

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(new ErrorInfo("123", "dummy"));
    }

    [Fact]
    public void Convert_to_Either() {
        var outcome = SuccessOutcome(42);

        var result = outcome.ToEither();

        result.IsRight.Should().BeTrue();
        result.GetRight().Should().Be(42);
    }

    [Fact]
    public void Convert_to_Either_error() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.ToEither();

        result.IsLeft.Should().BeTrue();
        result.GetLeft().Should().Be(new ErrorInfo("123", "dummy"));
    }

    #endregion

    #region Monad operations

    [Fact]
    public void Map_value_with_outcome() {
        var outcome = SuccessOutcome(42);

        var result = (from a in outcome
                      select a + 1
                     );

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42 + 1);
    }

    [Fact]
    public void Map_error_with_outcome() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.MapFailure(e => new ErrorInfo("456", e.Message));

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(new ErrorInfo("456", "dummy"));
    }

    [Fact]
    public void Binding_sync_with_sync() {
        var result = from a in SuccessOutcome(42)
                     from b in SuccessOutcome(a + 1)
                     select b;

        result.Should().Be(SuccessOutcome(43));
    }

    [Fact]
    public void BiMap_success() {
        var outcome = SuccessOutcome(42);

        var result = outcome.BiMap(v => v + 1, e => new ErrorInfo("123", e.Message));

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42 + 1);
    }

    [Fact]
    public void BiMap_failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.BiMap(v => v + 1, e => new ErrorInfo("456", e.Message));

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(new ErrorInfo("456", "dummy"));
    }

    [Fact]
    public void Match_success() {
        var outcome = SuccessOutcome("42");

        var result = outcome.Match(int.Parse, _ => 0);

        result.Should().Be(42);
    }

    [Fact]
    public void Match_failure() {
        var outcome = FailedOutcome<string>(new ErrorInfo("123", "dummy"));

        var result = outcome.Match(int.Parse, _ => 0);

        result.Should().Be(0);
    }

    #endregion

    #region Catch

    [Fact]
    public void Catch_And_SuccessOutcome() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.Catch(_ => 42);

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(42);
    }

    [Fact]
    public void Catch_And_Failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.Catch(_ => new ErrorInfo("456", "another dummy"));

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Should().Be(new ErrorInfo("456", "another dummy"));
    }

    [Fact]
    public void Catch_failure_outcome_with_another_outcome_returns_catch_outcome() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));
        var expected = new ErrorInfo("123", "another dummy");

        var result = a.Catch(_ => expected);

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be("123");
    }

    #endregion

    #region IfFail / IfSuccess

    [Fact]
    public void Get_default_value_from_failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.IfFail(42);

        result.Should().Be(42);
    }

    [Fact]
    public void Get_default_value_by_function_from_failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.IfFail(e => int.Parse(e.Code));

        result.Should().Be(123);
    }

    [Fact]
    public void Perform_action_if_failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var success = false;
        outcome.IfFail(_ => success = true);

        success.Should().BeTrue();
    }

    [Fact]
    public void Extract_values_and_success_state_from_success_outcome() {
        var outcome = SuccessOutcome(42);

        var success = outcome.IfSuccess(out var v, out _);

        success.Should().BeTrue();
        v.Should().Be(42);
    }

    [Fact]
    public void Extract_values_and_success_state_from_failure_outcome() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var success = outcome.IfSuccess(out _, out var e);

        success.Should().BeFalse();
        e.Should().Be(new ErrorInfo("123", "dummy"));
    }

    [Fact]
    public void Extract_values_and_failure_state_from_success_outcome() {
        var outcome = SuccessOutcome(42);

        var success = outcome.IfFail(out _, out var v);

        success.Should().BeFalse();
        v.Should().Be(42);
    }

    [Fact]
    public void Extract_values_and_failure_state_from_failure_outcome() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var success = outcome.IfFail(out var e, out _);

        success.Should().BeTrue();
        e.Should().Be(new ErrorInfo("123", "dummy"));
    }

    #endregion

    #region Pipe

    [Fact]
    public void Pipe_two_success_outcomes_returns_first() {
        var a = SuccessOutcome(42);
        var b = SuccessOutcome(123);

        var result = a | b;

        result.Should().Be(a);
    }

    [Fact]
    public void Pipe_two_failure_outcomes_returns_second() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));
        var b = FailedOutcome<int>(new ErrorInfo("123", "another dummy"));

        var result = a | b;

        result.Should().Be(b);
    }

    [Fact]
    public void Pipe_first_failure_outcome_with_second_success_outcome_returns_second() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));
        var b = SuccessOutcome(123);

        var result = a | b;

        result.Should().BeEquivalentTo(b);
    }

    [Fact]
    public void Pipe_first_failure_outcome_with_success_catch_returns_catch_value() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = (a | @catch(_ => 123));

        result.IsSuccess.Should().BeTrue();
        result.Unwrap().Should().Be(123);
    }

    [Fact]
    public void Pipe_first_failure_outcome_with_failure_catch_returns_catch_value() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = (a | @catch<int>(_ => new ErrorInfo("123", "another dummy")));

        result.IsFail.Should().BeTrue();
        result.UnwrapError().Code.Should().Be("123");
    }

    [Fact]
    public void Pipe_failure_outcome_and_perform_side_effect() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var success = false;
        var noChange = true;
        _ = a | failDo(_ => success = true);
        _ = a | @do<int>(_ => noChange = false);

        success.Should().BeTrue();
        noChange.Should().BeTrue();
    }

    [Fact]
    public void Pipe_success_outcome_and_perform_side_effect() {
        var a = SuccessOutcome(42);

        var success = false;
        var noChange = true;
        _ = a | failDo(_ => noChange = false);
        _ = a | @do<int>(_ => success = true);

        success.Should().BeTrue();
        noChange.Should().BeTrue();
    }

    [Fact]
    public void Pipe_failure_outcome_and_catch_for_sideeffect() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

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
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = (a | @catch<int>(new ErrorInfo("42", "any text"), 123));

        result.Should().Be(SuccessOutcome(123));
    }

    [Fact]
    public void Pipe_failure_outcome_is_caught_and_replaced_with_another_error() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = (a | @catch<int>(new ErrorInfo("42", "any text"), new ErrorInfo("123", "another dummy")));

        result.Should().Be(FailedOutcome<int>(new ErrorInfo("123", "another dummy")));
    }

    [Fact]
    public void Pipe_failure_outcome_is_caught_and_replaced_with_value_by_function() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = (a | @catch(new ErrorInfo("42", "any text"), e => int.Parse(e.Code) + 1));

        result.Should().Be(SuccessOutcome(43));
    }

    [Fact]
    public void Pipe_failure_outcome_is_caught_and_replaced_with_another_error_by_function() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = (a | @catch<int>(new ErrorInfo("42", "any text"),
                          e => new ErrorInfo((int.Parse(e.Code) + 1).ToString(), e.Message)));

        result.Should().Be(FailedOutcome<int>(new ErrorInfo("43", "dummy")));
    }

    [Fact]
    public void Pipe_success_outcome_and_perform_side_effect_work() {
        var a = SuccessOutcome(42);

        var result = 0;
        _ = a | @do<int>(v => {
                             result = v + 1;
                             return unit;
                         });

        result.Should().Be(43);
    }

    [Fact]
    public void Perform_side_effect_when_error() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = 0;
        _ = a | @failDo(e => result = int.Parse(e.Code) + 1);

        result.Should().Be(43);
    }

    [Fact]
    public void Pipe_unit_outcome_with_iffail_condition_should_not_get_called() {
        var called = false;

        _ = UnitOutcome | @failDo(_ => called = true);

        called.Should().BeFalse();
    }

    #endregion

    #region Serializable!

    sealed record TestRecord(string Name, int Age);

    [Fact]
    public void SerializeSuccessOutcomeToJson() {
        Outcome<TestRecord> outcome = new TestRecord("John", 42);

        // when
        var json = JsonSerializer.Serialize(outcome, new JsonSerializerOptions().UseRzConverters());

        // then
        json.Should().Be("{\"Data\":{\"Name\":\"John\",\"Age\":42}}");
    }

    [Fact]
    public void SerializeFailureOutcomeToJson() {
        Outcome<TestRecord> outcome = new ErrorInfo(StandardErrorCodes.Unhandled);

        // when
        var json = JsonSerializer.Serialize(outcome, new JsonSerializerOptions().UseRzConverters());

        // then
        json.Should().Be("{\"Error\":{\"Code\":\"unhandled\",\"Message\":\"unhandled\",\"TraceId\":null,\"DebugInfo\":null,\"Data\":null}}");
    }

    [Fact]
    public void DeserializeSuccessOutcomeToJson() {
        var json = "{\"Data\":{\"Name\":\"John\",\"Age\":42}}";
        Outcome<TestRecord> expected = new TestRecord("John", 42);

        // when
        var outcome = JsonSerializer.Deserialize<Outcome<TestRecord>>(json, new JsonSerializerOptions().UseRzConverters());

        // then
        outcome.Should().Be(expected);
    }

    [Fact]
    public void DeserializeFailureOutcomeToJson() {
        var json = "{\"Error\":{\"Code\":\"unhandled\",\"Message\":\"unhandled\",\"TraceId\":null,\"DebugInfo\":null,\"Data\":null}}";
        Outcome<TestRecord> expected = new ErrorInfo(StandardErrorCodes.Unhandled);

        // when
        var outcome = JsonSerializer.Deserialize<Outcome<TestRecord>>(json, new JsonSerializerOptions().UseRzConverters());

        // then
        outcome.Should().Be(expected);
    }

    #endregion
}