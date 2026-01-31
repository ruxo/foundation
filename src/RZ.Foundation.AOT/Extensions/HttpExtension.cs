namespace RZ.Foundation.Extensions;

public static class HttpExtension
{
    public static async ValueTask<Outcome<HttpResponseMessage>> TrySend(this HttpClient http, HttpRequestMessage request) {
        try{
            return await http.SendAsync(request);
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }

    extension(HttpContent content)
    {
        [PublicAPI]
        public async ValueTask<Outcome<byte[]>> ReadAsByteArray() {
            try{
                return await content.ReadAsByteArrayAsync();
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<Stream>> ReadStream() {
            try{
                return await content.ReadAsStreamAsync();
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<string>> ReadAsString() {
            try{
                return await content.ReadAsStringAsync();
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }
    }

    [PublicAPI]
    public static async ValueTask<Outcome<Stream>> ToStream(this HttpResponseMessage r) {
        using (r)
            return r.IsSuccessStatusCode
                       ? Success(await r.Content.ReadStream(), out var v, out var e) ? v : e.Trace("Read stream failed")
                       : new ErrorInfo(StandardErrorCodes.HttpError, $"({r.StatusCode}) HTTP failed",
                                       data: ToJson(("StatusCode", r.StatusCode.ToString()),
                                                    ("ReasonPhrase", r.ReasonPhrase)));
    }

    extension(Task<HttpResponseMessage> task)
    {
        [PublicAPI]
        public async ValueTask<Outcome<Stream>> ToStream()
            => Fail(await TryCatch(task), out var e, out var r) ? e.Trace("Read response failed") : await r.ToStream();
    }
}