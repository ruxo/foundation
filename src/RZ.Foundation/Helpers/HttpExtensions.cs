using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RZ.Foundation.Types;

namespace RZ.Foundation.Helpers;

[PublicAPI]
public static class HttpExtensions
{
    extension(Task<HttpResponseMessage> task)
    {
        public async ValueTask<T> DeserializedJson<T>() {
            var response = await task;
            return response.IsSuccessStatusCode
                       ? (await response.Content.ReadFromJsonAsync<T>())!
                       : throw new ErrorInfoException(StandardErrorCodes.HttpError, $"({response.StatusCode}) HTTP failed",
                                                      data: new { response.StatusCode, response.ReasonPhrase });
        }

        public async ValueTask<Stream> ToStream() {
            using var r = await task;
            return r.IsSuccessStatusCode
                       ? await r.Content.ReadAsStreamAsync()
                       : throw new ErrorInfoException(StandardErrorCodes.HttpError, $"({r.StatusCode}) HTTP failed",
                                                      data: new { r.StatusCode, r.ReasonPhrase });
        }
    }
}