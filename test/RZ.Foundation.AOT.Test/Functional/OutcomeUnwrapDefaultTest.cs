using RZ.Foundation.Types;

namespace RZ.Foundation.Functional;

public sealed class OutcomeUnwrapDefaultTest
{
    [Test]
    public async ValueTask UnwrapOrDefault_returns_supplied_default_on_failure() {
        var failed = FailedOutcome<int>(new ErrorInfo("123", "missing"));

        var result = failed.UnwrapOrDefault(42);

        await Assert.That(result).IsEqualTo(42);
    }

    [Test]
    public async ValueTask UnwrapOrDefault_returns_data_on_success() {
        var success = SuccessOutcome(7);

        var result = success.UnwrapOrDefault(42);

        await Assert.That(result).IsEqualTo(7);
    }

    [Test]
    public async ValueTask UnwrapErrorOrDefault_returns_error_on_failure() {
        var error = new ErrorInfo("123", "missing");
        var failed = FailedOutcome<int>(error);

        var fallback = new ErrorInfo("999", "none");
        var result = failed.UnwrapErrorOrDefault(fallback);

        await Assert.That(result).IsEqualTo(error);
    }

    [Test]
    public async ValueTask UnwrapErrorOrDefault_returns_supplied_default_on_success() {
        var success = SuccessOutcome(5);

        var fallback = new ErrorInfo("999", "none");
        var result = success.UnwrapErrorOrDefault(fallback);

        await Assert.That(result).IsEqualTo(fallback);
    }
}
