using System.Diagnostics.CodeAnalysis;

namespace RZ.Foundation.Extensions;

public static class HttpExtension
{
    extension(HttpClient http)
    {
        [PublicAPI]
        public async ValueTask<Outcome<HttpResponseMessage>> Get([StringSyntax("Uri"), UriString("GET")] string? requestUri, CancellationToken cancel = default) {
            try{
                return await http.GetAsync(requestUri, cancel);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<HttpResponseMessage>> Post([StringSyntax("Uri"), UriString("POST")] string? requestUri, HttpContent? content, CancellationToken cancel = default) {
            try{
                return await http.PostAsync(requestUri, content, cancel);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<HttpResponseMessage>> Put([StringSyntax("Uri"), UriString("PUT")] string? requestUri, HttpContent? content, CancellationToken cancel = default) {
            try{
                return await http.PutAsync(requestUri, content, cancel);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<HttpResponseMessage>> Patch([StringSyntax("Uri"), UriString("PATCH")] string? requestUri, HttpContent? content, CancellationToken cancel = default) {
            try{
                return await http.PatchAsync(requestUri, content, cancel);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public async ValueTask<Outcome<HttpResponseMessage>> Delete([StringSyntax("Uri"), UriString("DELETE")] string? requestUri, CancellationToken cancel = default) {
            try{
                return await http.DeleteAsync(requestUri, cancel);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        public async ValueTask<Outcome<HttpResponseMessage>> TrySend(HttpRequestMessage request) {
            try{
                return await http.SendAsync(request);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
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

    extension(HttpResponseMessage r)
    {
        async ValueTask<Outcome<T>> Read<T>(Func<HttpContent,ValueTask<Outcome<T>>> reader, string readError) {
            using (r)
                return r.IsSuccessStatusCode
                           ? Success(await reader(r.Content), out var v, out var e) ? v : e.Trace(readError)
                           : new ErrorInfo(HttpError, $"({r.StatusCode}) HTTP failed",
                                           data: ToJson(("StatusCode", r.StatusCode.ToString()),
                                                        ("ReasonPhrase", r.ReasonPhrase)));
        }

        [PublicAPI]
        public ValueTask<Outcome<string>> ReadString() => r.Read(c => c.ReadAsString(), "Read string failed");

        [PublicAPI]
        public ValueTask<Outcome<byte[]>> ReadByteArray() => r.Read(c => c.ReadAsByteArray(), "Read byte array failed");

        [PublicAPI]
        public ValueTask<Outcome<Stream>> ReadStream() => r.Read(c => c.ReadStream(), "Read stream failed");
    }

    extension(ValueTask<Outcome<HttpResponseMessage>> task)
    {
        async ValueTask<Outcome<T>> Read<T>(Func<HttpResponseMessage,ValueTask<Outcome<T>>> reader) {
            try{
                if (Fail(await task, out var e, out var r)) return e.Trace("Read response failed");
                using(r)
                    return await reader(r);
            }
            catch (Exception e){
                return ErrorFrom.Exception(e);
            }
        }

        [PublicAPI]
        public ValueTask<Outcome<string>> ReadString() => task.Read(r => r.ReadString());

        [PublicAPI]
        public ValueTask<Outcome<byte[]>> ReadByteArray() => task.Read(r => r.ReadByteArray());

        [PublicAPI]
        public ValueTask<Outcome<Stream>> ReadStream() => task.Read(r => r.ReadStream());
    }

    [PublicAPI]
    public static ValueTask<Outcome<Stream>> ReadStream(this Task<HttpResponseMessage> task)
        => TryCatch(async () => await task).ReadStream();
}