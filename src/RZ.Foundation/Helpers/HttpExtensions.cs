using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using RZ.Foundation.Types;

namespace RZ.Foundation.Helpers;

public static class HttpExtensions
{
    extension(Task<HttpResponseMessage> task)
    {
        [PublicAPI]
        public async ValueTask<Outcome<T>> DeserializedJson<T>() {
            var r = await task;
            return r.IsSuccessStatusCode
                       ? (await r.Content.ReadFromJsonAsync<T>())!
                       : new ErrorInfo(StandardErrorCodes.HttpError, $"({r.StatusCode}) HTTP failed",
                                       data: AOT.Prelude.ToJson(("StatusCode", r.StatusCode.ToString()),
                                                                ("ReasonPhrase", r.ReasonPhrase)));
        }

        [PublicAPI]
        public async ValueTask<Outcome<Stream>> ToStream() {
            using var r = await task;
            return r.IsSuccessStatusCode
                       ? await r.Content.ReadAsStreamAsync()
                       : new ErrorInfo(StandardErrorCodes.HttpError, $"({r.StatusCode}) HTTP failed",
                                       data: AOT.Prelude.ToJson(("StatusCode", r.StatusCode.ToString()),
                                                                ("ReasonPhrase", r.ReasonPhrase)));
        }
    }
}