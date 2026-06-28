using System.Text.Json.Serialization;

namespace RZ.Foundation.Types;

/// <summary>
/// Represents the data required to perform a web request, including the URI, HTTP method, headers, and optional body content.
/// </summary>
public record WebRequestData(Uri Uri)
{
    public WebRequestData(string uri) : this(new Uri(uri)) { }

    [JsonConstructor]
    public WebRequestData(Uri uri, string method, string[][] headers, string? body) : this(uri) {
        Method = method;
        Headers = headers;
        Body = body;
    }

    public string Method { get; init; } = "GET";
    public string[][] Headers { get; init; } = [];
    public string? Body { get; init; }

    public Outcome<HttpRequestMessage> ToHttpRequest() {
        if (ToHttpMethod(Method).UnlessSome(out var method))
            return new ErrorInfo(INVALID_RESPONSE, $"Unsupported HTTP method: {Method}");

        var req = new HttpRequestMessage(method, Uri) {
            Content = Optional(Body).Map(x => new StringContent(x)).ToNullable()
        };
        foreach (var h in Headers){
            if (h.Length < 2)
                return ErrorInfo.New(INVALID_REQUEST, $"Malformed header row: [{string.Join(", ", h)}]");

            // TryAddWithoutValidation falls back to content headers for content-only names (e.g. Content-Type)
            if (!req.Headers.TryAddWithoutValidation(h[0], h[1]))
                req.Content?.Headers.TryAddWithoutValidation(h[0], h[1]);
        }
        return req;
    }

    static Option<HttpMethod> ToHttpMethod(string method)
        => method switch {
            "GET"    => HttpMethod.Get,
            "POST"   => HttpMethod.Post,
            "PUT"    => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "PATCH"  => HttpMethod.Patch,

            _ => None
        };
}

[PublicAPI]
public static class WebRequestDataExtension
{
    public static async ValueTask<Outcome<(string MimeType, byte[] Data)>> Retrieve(this WebRequestData imageRequest, HttpClient http) {
        if (Fail(imageRequest.ToHttpRequest(), out var e, out var request)
         || Fail(await http.TrySend(request), out e, out var response))
            return e;

        if (response.IsSuccessStatusCode){
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            if (Fail(await response.Content.ReadAsByteArray(), out e, out var data)) return e;

            return (contentType, data);
        }
        if (Fail(await response.Content.ReadAsString(), out e, out var error)) return e;

        return new ErrorInfo(SERVICE_ERROR, $"Get image failed: {error}",
                             data: ToJson(("StatusCode", response.StatusCode.ToString())));
    }
}