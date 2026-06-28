using LanguageExt;
using LanguageExt.Common;
using RZ.Foundation.Extensions;
using RZ.Foundation.Types;

namespace RZ.Foundation.Functional;

public sealed class OutcomeTest
{
    #region General

    [Test]
    public async ValueTask OutcomeDirectSuccessAssignment() {
        var outcome = SuccessOutcome(42);

        await Assert.That(outcome.IsSuccess).IsTrue();
        await Assert.That(outcome.IsFail).IsFalse();
        await Assert.That(outcome.Unwrap()).IsEqualTo(42);

        await Assert.That(() => outcome.UnwrapError()).Throws<InvalidOperationException>();
    }

    [Test]
    public async ValueTask OutcomeDirectFailureAssignment() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        await Assert.That(outcome.IsSuccess).IsFalse();
        await Assert.That(outcome.IsFail).IsTrue();
        await Assert.That(outcome.UnwrapError().Is("123")).IsTrue();

        await Assert.That(() => outcome.Unwrap()).Throws<ErrorInfoException>();
    }

    [Test]
    public async ValueTask Outcome_success_equality() {
        var a = SuccessOutcome(42);
        var b = SuccessOutcome(42);

        var result = a.Equals(b);

        await Assert.That(result).IsTrue();
    }

    [Test]
    public async ValueTask Convert_from_error() {
        var err = Error.New(123, "dummy");

        Outcome<string> result = err;

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Code).IsEqualTo(StandardErrorCodes.UNHANDLED);
    }

    [Test]
    public async ValueTask Convert_from_either_error() {
        Either<ErrorInfo, string> err = new ErrorInfo(StandardErrorCodes.TIMEOUT, "dummy");

        Outcome<string> result = err;

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Code).IsEqualTo(StandardErrorCodes.TIMEOUT);
    }

    [Test]
    public async ValueTask Convert_from_either_success() {
        Either<ErrorInfo, string> err = "dummy";

        Outcome<string> result = err;

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Unwrap()).IsEqualTo("dummy");
    }

    [Test]
    public async ValueTask Default_value_for_failure() {
        var value = FailedOutcome<int>(new ErrorInfo(StandardErrorCodes.UNHANDLED, "dummy"));

        var result = value.IfFail(123);

        await Assert.That(result).IsEqualTo(123);
    }

    #endregion

    #region From other monads

    [Test]
    public async ValueTask From_option_some() {
        Option<int> option = 42;

        var result = option.ToOutcome(new ErrorInfo("123", "dummy"));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Unwrap()).IsEqualTo(42);
    }

    [Test]
    public async ValueTask From_option_none() {
        Option<int> option = Option<int>.None;

        var result = option.ToOutcome(new ErrorInfo("123", "dummy"));

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError()).IsEqualTo(new ErrorInfo("123", "dummy"));
    }

    [Test]
    public async ValueTask Convert_to_Either() {
        var outcome = SuccessOutcome(42);

        var result = outcome.ToEither();

        await Assert.That(result.IsRight).IsTrue();
        await Assert.That(result.GetRight()).IsEqualTo(42);
    }

    [Test]
    public async ValueTask Convert_to_Either_error() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.ToEither();

        await Assert.That(result.IsLeft).IsTrue();
        await Assert.That(result.GetLeft()).IsEqualTo(new ErrorInfo("123", "dummy"));
    }

    #endregion

    #region Monad operations

    [Test]
    public async ValueTask Map_value_with_outcome() {
        var outcome = SuccessOutcome(42);

        var result = from a in outcome
                     select a + 1;

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Unwrap()).IsEqualTo(42 + 1);
    }

    [Test]
    public async ValueTask Map_error_with_outcome() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.MapFailure(e => new ErrorInfo("456", e.Message));

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError()).IsEqualTo(new ErrorInfo("456", "dummy"));
    }

    [Test]
    public async ValueTask Binding_sync_with_sync() {
        var result = from a in SuccessOutcome(42)
                     from b in SuccessOutcome(a + 1)
                     select b;

        await Assert.That(result).IsEqualTo(SuccessOutcome(43));
    }

    [Test]
    public async ValueTask Binding_async_with_let() {
        var result = from a in new ValueTask<Outcome<int>>(42)
                     from b in new ValueTask<Outcome<int>>(a + 1)
                     let c = b + 1
                     from d in new ValueTask<Outcome<int>>(c + 1)
                     select d;

        await Assert.That(await result).IsEqualTo(SuccessOutcome(45));
    }

    [Test]
    public async ValueTask Binding_async_with_sync_and_async() {
        var result = from a in new ValueTask<Outcome<int>>(SuccessOutcome(42))
                     from b in SuccessOutcome(a + 1)
                     from c in new ValueTask<Outcome<int>>(SuccessOutcome(b + 1))
                     select c;

        await Assert.That(await result).IsEqualTo(SuccessOutcome(44));
    }

    [Test]
    public async ValueTask Binding_sync_with_async_and_async() {
        var result = from a in SuccessOutcome(42)
                     from b in new ValueTask<Outcome<int>>(SuccessOutcome(a + 1))
                     from c in new ValueTask<Outcome<int>>(SuccessOutcome(b + 1))
                     select c;

        await Assert.That(await result).IsEqualTo(SuccessOutcome(44));
    }

    [Test]
    public async ValueTask Binding_sync_with_async_and_sync() {
        var result = from a in SuccessOutcome(42)
                     from b in new ValueTask<Outcome<int>>(SuccessOutcome(a + 1))
                     from c in SuccessOutcome(b + 1)
                     select c;

        await Assert.That(await result).IsEqualTo(SuccessOutcome(44));
    }

    [Test]
    public async ValueTask BiMap_success() {
        var outcome = SuccessOutcome(42);

        var result = outcome.BiMap(v => v + 1, e => new ErrorInfo("123", e.Message));

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Unwrap()).IsEqualTo(42 + 1);
    }

    [Test]
    public async ValueTask BiMap_failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.BiMap(v => v + 1, e => new ErrorInfo("456", e.Message));

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError()).IsEqualTo(new ErrorInfo("456", "dummy"));
    }

    [Test]
    public async ValueTask Match_success() {
        var outcome = SuccessOutcome("42");

        var result = outcome.Match(int.Parse, _ => 0);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async ValueTask Match_failure() {
        var outcome = FailedOutcome<string>(new ErrorInfo("123", "dummy"));

        var result = outcome.Match(int.Parse, _ => 0);

        await Assert.That(result).IsEqualTo(0);
    }

    #endregion

    #region Catch

    [Test]
    public async ValueTask Catch_And_SuccessOutcome() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.Catch(_ => 42);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Unwrap()).IsEqualTo(42);
    }

    [Test]
    public async ValueTask Catch_And_Failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.Catch(_ => new ErrorInfo("456", "another dummy"));

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError()).IsEqualTo(new ErrorInfo("456", "another dummy"));
    }

    [Test]
    public async ValueTask Catch_failure_outcome_with_another_outcome_returns_catch_outcome() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));
        var expected = new ErrorInfo("123", "another dummy");

        var result = a.Catch(_ => expected);

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Code).IsEqualTo("123");
    }

    #endregion

    #region IfFail / IfSuccess

    [Test]
    public async ValueTask Get_default_value_from_failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.IfFail(42);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async ValueTask Get_default_value_by_function_from_failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var result = outcome.IfFail(e => int.Parse(e.Code));

        await Assert.That(result).IsEqualTo(123);
    }

    [Test]
    public async ValueTask Perform_action_if_failure() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var success = false;
        outcome.IfFail(_ => success = true);

        await Assert.That(success).IsTrue();
    }

    [Test]
    public async ValueTask Extract_values_and_success_state_from_success_outcome() {
        var outcome = SuccessOutcome(42);

        var success = outcome.IfSuccess(out var v, out _);

        await Assert.That(success).IsTrue();
        await Assert.That(v).IsEqualTo(42);
    }

    [Test]
    public async ValueTask Extract_values_and_success_state_from_failure_outcome() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var success = outcome.IfSuccess(out _, out var e);

        await Assert.That(success).IsFalse();
        await Assert.That(e).IsEqualTo(new ErrorInfo("123", "dummy"));
    }

    [Test]
    public async ValueTask Extract_values_and_failure_state_from_success_outcome() {
        var outcome = SuccessOutcome(42);

        var success = outcome.IfFail(out _, out var v);

        await Assert.That(success).IsFalse();
        await Assert.That(v).IsEqualTo(42);
    }

    [Test]
    public async ValueTask Extract_values_and_failure_state_from_failure_outcome() {
        var outcome = FailedOutcome<int>(new ErrorInfo("123", "dummy"));

        var success = outcome.IfFail(out var e, out _);

        await Assert.That(success).IsTrue();
        await Assert.That(e).IsEqualTo(new ErrorInfo("123", "dummy"));
    }

    #endregion

    #region Pipe

    [Test]
    public async ValueTask Pipe_two_success_outcomes_returns_first() {
        var a = SuccessOutcome(42);
        var b = SuccessOutcome(123);

        var result = a | b;

        await Assert.That(result).IsEqualTo(a);
    }

    [Test]
    public async ValueTask Pipe_two_failure_outcomes_returns_second() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));
        var b = FailedOutcome<int>(new ErrorInfo("123", "another dummy"));

        var result = a | b;

        await Assert.That(result).IsEqualTo(b);
    }

    [Test]
    public async ValueTask Pipe_first_failure_outcome_with_second_success_outcome_returns_second() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));
        var b = SuccessOutcome(123);

        var result = a | b;

        await Assert.That(result).IsEqualTo(b);
    }

    [Test]
    public async ValueTask Pipe_first_failure_outcome_with_success_catch_returns_catch_value() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = a | @catch(_ => 123);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Unwrap()).IsEqualTo(123);
    }

    [Test]
    public async ValueTask Pipe_first_failure_outcome_with_failure_catch_returns_catch_value() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = a | @catch<int>(_ => new ErrorInfo("123", "another dummy"));

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Code).IsEqualTo("123");
    }

    [Test]
    public async ValueTask Pipe_failure_outcome_and_perform_side_effect() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var success = false;
        var noChange = true;
        _ = a | failDo(_ => success = true);
        _ = a | @do<int>(_ => noChange = false);

        await Assert.That(success).IsTrue();
        await Assert.That(noChange).IsTrue();
    }

    [Test]
    public async ValueTask Pipe_success_outcome_and_perform_side_effect() {
        var a = SuccessOutcome(42);

        var success = false;
        var noChange = true;
        _ = a | failDo(_ => noChange = false);
        _ = a | @do<int>(_ => success = true);

        await Assert.That(success).IsTrue();
        await Assert.That(noChange).IsTrue();
    }

    [Test]
    public async ValueTask Pipe_failure_outcome_and_catch_for_sideeffect() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var success = false;
        Unit doSomething() {
            success = true;
            return unit;
        }

        _ = a | failDo(_ => doSomething());

        await Assert.That(success).IsTrue();
    }

    [Test]
    public async ValueTask Pipe_failure_outcome_is_caught_and_replaced_with_value() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = a | @catch<int>(new ErrorInfo("42", "any text"), 123);

        await Assert.That(result).IsEqualTo(SuccessOutcome(123));
    }

    [Test]
    public async ValueTask Pipe_failure_outcome_is_caught_and_replaced_with_another_error() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = a | @catch<int>(new ErrorInfo("42", "any text"), new ErrorInfo("123", "another dummy"));

        await Assert.That(result).IsEqualTo(FailedOutcome<int>(new ErrorInfo("123", "another dummy")));
    }

    [Test]
    public async ValueTask Pipe_failure_outcome_is_caught_and_replaced_with_value_by_function() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = a | @catch(new ErrorInfo("42", "any text"), e => int.Parse(e.Code) + 1);

        await Assert.That(result).IsEqualTo(SuccessOutcome(43));
    }

    [Test]
    public async ValueTask Pipe_failure_outcome_is_caught_and_replaced_with_another_error_by_function() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = a | @catch<int>(new ErrorInfo("42", "any text"),
                                     e => new ErrorInfo((int.Parse(e.Code) + 1).ToString(), e.Message));

        await Assert.That(result).IsEqualTo(FailedOutcome<int>(new ErrorInfo("43", "dummy")));
    }

    [Test]
    public async ValueTask Pipe_success_outcome_and_perform_side_effect_work() {
        var a = SuccessOutcome(42);

        var result = 0;
        _ = a | @do<int>(v => {
            result = v + 1;
            return unit;
        });

        await Assert.That(result).IsEqualTo(43);
    }

    [Test]
    public async ValueTask Perform_side_effect_when_error() {
        var a = FailedOutcome<int>(new ErrorInfo("42", "dummy"));

        var result = 0;
        _ = a | @failDo(e => result = int.Parse(e.Code) + 1);

        await Assert.That(result).IsEqualTo(43);
    }

    [Test]
    public async ValueTask Pipe_unit_outcome_with_iffail_condition_should_not_get_called() {
        var called = false;

        _ = UnitOutcome | @failDo(_ => called = true);

        await Assert.That(called).IsFalse();
    }

    #endregion

    [Test]
    public async ValueTask IfFail_Enum() {
        var x = new[] {SuccessOutcome(42), FailedOutcome<int>(ErrorInfo.NotFound), SuccessOutcome(99)};

        await Assert.That(x.IfAnyFail(out var e)).IsTrue();
        await Assert.That(e).IsEqualTo(ErrorInfo.NotFound);
    }
}