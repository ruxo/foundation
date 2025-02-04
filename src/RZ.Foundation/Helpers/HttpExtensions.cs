using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RZ.Foundation.Types;

namespace RZ.Foundation.Helpers;

public static class HttpExtensions
{
    public static async ValueTask<T> DeserializedJson<T>(this Task<HttpResponseMessage> task) {
        var response = await task;
        return response.IsSuccessStatusCode
                   ? (await response.Content.ReadFromJsonAsync<T>())!
                   : throw new ErrorInfoException(StandardErrorCodes.HttpError, $"({response.StatusCode}) HTTP failed",
                                                  data: new { response.StatusCode, response.ReasonPhrase });
    }

    public static async ValueTask<Stream> ToStream(this Task<HttpResponseMessage> task) {
        using var r = await task;
        return r.IsSuccessStatusCode
                   ? await r.Content.ReadAsStreamAsync()
                   : throw new ErrorInfoException(StandardErrorCodes.HttpError, $"({r.StatusCode}) HTTP failed",
                                                  data: new { r.StatusCode, r.ReasonPhrase });
    }
}