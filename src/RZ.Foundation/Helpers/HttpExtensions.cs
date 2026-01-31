using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using RZ.Foundation.Types;

namespace RZ.Foundation.Helpers;

public static class HttpExtensions
{
    [PublicAPI]
    public static async ValueTask<Outcome<T>> DeserializedJson<T>(this HttpResponseMessage r, JsonSerializerOptions? options = null) {
        using (r)
            return r.IsSuccessStatusCode
                       ? Success(await TryCatch(r.Content.ReadFromJsonAsync<T>(options)), out var v, out var e) ? v : e.Trace($"Deserialize to {typeof(T).Name} failed")
                       : new ErrorInfo(StandardErrorCodes.HttpError, $"({r.StatusCode}) HTTP failed",
                                       data: ToJson(("StatusCode", r.StatusCode.ToString()),
                                                    ("ReasonPhrase", r.ReasonPhrase)));
    }

    [PublicAPI]
    public static async ValueTask<Outcome<T>> DeserializedJson<T>(this Task<HttpResponseMessage> task, JsonSerializerOptions? options = null)
        => Fail(await TryCatch(task), out var e, out var r) ? e.Trace("Read response failed") : await r.DeserializedJson<T>(options);
}