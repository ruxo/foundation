using RZ.Foundation.Extensions;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RZ.Foundation.Net
{
    public interface ITextHttp
    {
        Task<ApiResult<string>> Request(HttpMethod method, Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<ApiResult<string>> Get(Uri uri, Option<HttpRequestOption> config);
        Task<ApiResult<string>> Post(Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<ApiResult<string>> Put(Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<ApiResult<string>> Delete(Uri uri, Option<HttpRequestOption> config);
    }
    /// <summary>
    /// Text HTTP is a HTTP requester that specially works with string as input and output.
    /// </summary>
    public class TextHttp : ITextHttp
    {
        static readonly MediaTypeWithQualityHeaderValue JsonMimeType = new MediaTypeWithQualityHeaderValue("application/json");
        public async Task<ApiResult<string>> Request( HttpMethod method
                                                           , Uri uri
                                                           , Option<string> data
                                                           , Option<HttpRequestOption> config) {
            var req = new HttpRequestMessage(method, uri);
            data.Apply(text =>
            {
                req.Content = new StringContent(text);
                req.Content.Headers.ContentType = JsonMimeType;
            });
            config.Apply(ApplyConfig(req));

            using(var http = new HttpClient())
            {
                var res = await http.SendAsync(req);
                var text = await res.Content.ReadAsStringAsync();
                return res.IsSuccessStatusCode
                     ? text.AsApiSuccess()
                     : ExceptionExtension.CreateError(res.ReasonPhrase, $"http-{(int)res.StatusCode}", uri.ToString(), text);
            }
        }

        public Task<ApiResult<string>> Get(Uri uri, Option<HttpRequestOption> config) => Request(HttpMethod.Get, uri, null, config);
        public Task<ApiResult<string>> Post(Uri uri, Option<string> data, Option<HttpRequestOption> config) => Request(HttpMethod.Post, uri, data, config);
        public Task<ApiResult<string>> Put(Uri uri, Option<string> data, Option<HttpRequestOption> config) => Request(HttpMethod.Post, uri, data, config);
        public Task<ApiResult<string>> Delete(Uri uri, Option<HttpRequestOption> config) => Request(HttpMethod.Post, uri, null, config);

        static Action<HttpRequestOption> ApplyConfig(HttpRequestMessage req) => config =>
            config.Authentication.Apply(auth => 
                req.Headers.Authorization = auth.Parameter.Get( p => new AuthenticationHeaderValue(auth.Scheme, p)
                                                              , () => new AuthenticationHeaderValue(auth.Scheme)));
    }
}
