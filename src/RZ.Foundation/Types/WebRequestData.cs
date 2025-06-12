using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;

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

    public HttpRequestMessage ToHttpRequest() {
        var req = new HttpRequestMessage(ToHttpMethod(Method), Uri) {
            Content = Optional(Body).Map(x => new StringContent(x)).ToNullable()
        };
        Headers.Iter(h => req.Headers.Add(h[0], h[1]));
        return req;
    }

    static HttpMethod ToHttpMethod(string method)
        => method switch {
            "GET"    => HttpMethod.Get,
            "POST"   => HttpMethod.Post,
            "PUT"    => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "PATCH"  => HttpMethod.Patch,

            _ => throw new NotSupportedException($"Invalid HTTP method: {method}")
        };
}

[PublicAPI]
public static class WebRequestDataExtension
{
    public static async Task<(string MimeType, byte[] Data)> Retrieve(this WebRequestData imageRequest, HttpClient http) {
        var response = await http.SendAsync(imageRequest.ToHttpRequest());
        if (response.IsSuccessStatusCode){
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var data = await response.Content.ReadAsByteArrayAsync();
            return (contentType, data);
        }
        var error = await response.Content.ReadAsStringAsync();
        throw new ErrorInfoException(StandardErrorCodes.ServiceError, $"Get image failed: {error}", data: new { response.StatusCode });
    }
}