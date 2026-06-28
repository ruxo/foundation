using System.Net;

namespace RZ.Foundation.Extensions;

public sealed class HttpTrySendTest
{
    [Test]
    public async ValueTask TrySend_forwards_cancellation_token_and_returns_failed_outcome() {
        var handler = new CapturingHandler();
        using var http = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/slow");

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await http.TrySend(request, cts.Token);

        // The cancellation surfaces as a caught exception converted into a failed Outcome.
        await Assert.That(result.IsFail).IsTrue();
        // The token must have been forwarded to the underlying SendAsync call.
        await Assert.That(handler.ReceivedToken.IsCancellationRequested).IsTrue();
    }

    [Test]
    public async ValueTask TrySend_without_token_succeeds() {
        var handler = new CapturingHandler();
        using var http = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/ok");

        var result = await http.TrySend(request);

        await Assert.That(result.IsSuccess).IsTrue();
        // (Note: the handler's received token may still be cancelable — HttpClient links its own
        //  request-timeout CTS — so we only assert the call succeeded without a caller token.)
    }

    sealed class CapturingHandler : HttpMessageHandler
    {
        public CancellationToken ReceivedToken { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            ReceivedToken = cancellationToken;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
