using JetBrains.Annotations;

namespace RZ.Foundation.Types;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class WebRequestDataTest
{
    [Test]
    public async ValueTask Malformed_header_row_returns_failed_outcome_instead_of_throwing() {
        // A header row with a single element used to throw IndexOutOfRangeException.
        var bad = new WebRequestData(new Uri("https://example.com")) {
            Headers = [["Authorization"]] // missing value element
        };

        var result = bad.ToHttpRequest();

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is(StandardErrorCodes.InvalidRequest)).IsTrue();
    }

    [Test]
    public async ValueTask Content_header_is_handled_without_unhandled_exception() {
        // A content header (Content-Type) on the request-header collection used to throw
        // InvalidOperationException. It must now be routed to the content headers and succeed.
        var request = new WebRequestData(new Uri("https://example.com")) {
            Method  = "POST",
            Body    = "{}",
            Headers = [["Content-Type", "application/json"]]
        };

        var result = request.ToHttpRequest();

        // The key requirement: no unhandled exception escapes; the Outcome succeeds.
        await Assert.That(result.IsSuccess).IsTrue();

        using var message = result.Unwrap();
        // Content headers belong on the content, not the request-header collection.
        // (Querying a content-header name on request Headers itself throws, so we only
        //  assert it is present on the content headers.)
        await Assert.That(message.Content!.Headers.Contains("Content-Type")).IsTrue();
    }

    [Test]
    public async ValueTask Valid_request_header_is_applied_to_request_headers() {
        // Regression guard: ordinary request-level headers keep working.
        var request = new WebRequestData(new Uri("https://example.com")) {
            Headers = [["Authorization", "Bearer token"]]
        };

        var result = request.ToHttpRequest();

        await Assert.That(result.IsSuccess).IsTrue();

        using var message = result.Unwrap();
        await Assert.That(message.Headers.GetValues("Authorization").Single()).IsEqualTo("Bearer token");
    }

    [Test]
    public async ValueTask Unsupported_method_returns_failed_outcome() {
        // Existing behaviour for valid inputs must be preserved.
        var request = new WebRequestData(new Uri("https://example.com")) {
            Method = "TRACE"
        };

        var result = request.ToHttpRequest();

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is(StandardErrorCodes.InvalidResponse)).IsTrue();
    }
}
