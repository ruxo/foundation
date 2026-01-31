using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RZ.Foundation.Types;

namespace RZ.Foundation.Extensions;

public static class HttpExtensions
{
    extension(HttpClient http)
    {
        [PublicAPI]
        public async ValueTask<Outcome<HttpResponseMessage>> PostJson<T>([StringSyntax("Uri"), UriString("POST")] string? requestUri,
                                                                         T body,
                                                                         JsonSerializerOptions? options = null,
                                                                         CancellationToken cancel = default) {
            try{
                return await http.PostAsJsonAsync(requestUri, body, options ?? RzRecommendedJsonOptions, cancel);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<HttpResponseMessage>> PutJson<T>([StringSyntax("Uri"), UriString("PUT")] string? requestUri,
                                                                        T body,
                                                                        JsonSerializerOptions? options = null,
                                                                        CancellationToken cancel = default) {
            try{
                return await http.PutAsJsonAsync(requestUri, body, options ?? RzRecommendedJsonOptions, cancel);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<HttpResponseMessage>> PatchJson<T>([StringSyntax("Uri"), UriString("PATCH")] string? requestUri,
                                                                          T body,
                                                                          JsonSerializerOptions? options = null,
                                                                          CancellationToken cancel = default) {
            try{
                return await http.PatchAsJsonAsync(requestUri, body, options ?? RzRecommendedJsonOptions, cancel);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }
    }

    extension(HttpResponseMessage r)
    {
        /// <summary>
        /// Deserializes the response body as JSON. Note that the response will be disposed.
        /// </summary>
        [PublicAPI]
        public async ValueTask<Outcome<T>> DeserializedJson<T>(JsonSerializerOptions? options = null) {
            using (r)
                return r.IsSuccessStatusCode
                           ? Success(await TryCatch(r.Content.ReadFromJsonAsync<T>(options)), out var v, out var e) ? v : e.Trace($"Deserialize to {typeof(T).Name} failed")
                           : new ErrorInfo(HttpError, $"({r.StatusCode}) HTTP failed",
                                           data: ToJson(("StatusCode", r.StatusCode.ToString()),
                                                        ("ReasonPhrase", r.ReasonPhrase)));
        }

        /// <summary>
        /// Asserts that the response is successful and returns the deserialized body. Note that the response will be disposed.
        /// </summary>
        [PublicAPI]
        public async ValueTask<Outcome<T>> ExpectJson<T>(JsonSerializerOptions? options = null) {
            using var _ = r;
            ErrorInfo? e;
            if (r.IsSuccessStatusCode)
                return Success(await r.DeserializedJson<T>(options), out var v, out e)
                           ? v
                           : e.Wrap(InvalidResponse, "Invalid response");

            return Fail(await r.Content.ReadAsString(), out e, out var body) ? e.Trace("Read response failed") : ThrowError<T>(body, options);
        }

        /// <summary>
        /// Asserts that the response is successful and does not contain a body. Note that the response will be disposed.
        /// </summary>
        [PublicAPI]
        public async ValueTask<Outcome<Unit>> MustSucceed(JsonSerializerOptions? options = null) {
            using var _ = r;
            if (Fail(await r.Content.ReadAsString(), out var e, out var body)) return e.Trace("From HTTP response");

            if (r.IsSuccessStatusCode){
                if (body.Length > 0)
                    return new ErrorInfo(ValidationFailed, "Does not expect any response body.", data: body);
                return unit;
            }
            return ThrowError<Unit>(body, options);
        }
    }

    static Outcome<T> ThrowError<T>(string body, JsonSerializerOptions? options) {
        if (Success(JsonDeserialize<ErrorInfo>(body, options), out var errorInfo))
            if (!string.IsNullOrEmpty(errorInfo.Code))
                return errorInfo.Trace("From HTTP response");
        return new ErrorInfo(HttpError, data: body);
    }

    [PublicAPI]
    public static async ValueTask<Outcome<T>> DeserializedJson<T>(this ValueTask<Outcome<HttpResponseMessage>> task, JsonSerializerOptions? options = null) {
        if (Fail(await task, out var e, out var r))
            return e.Trace("Read response failed");
        using (r)
            return await r.DeserializedJson<T>(options);
    }

    [PublicAPI]
    public static ValueTask<Outcome<T>> DeserializedJson<T>(this Task<HttpResponseMessage> task, JsonSerializerOptions? options = null)
        => TryCatch(async () => await task).DeserializedJson<T>(options);
}