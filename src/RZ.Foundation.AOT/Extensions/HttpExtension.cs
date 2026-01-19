using RZ.Foundation.Types;

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
        public async ValueTask<Outcome<Stream>> ReadAsStream() {
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
}