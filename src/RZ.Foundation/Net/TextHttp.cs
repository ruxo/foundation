using RZ.Foundation.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static RZ.Foundation.OptionHelper;

namespace RZ.Foundation.Net
{
    public interface IHttp
    {
        string NetworkErrorCode { get; }
        Option<HttpStatusCode> ParseHttpError(string httpErrorCode);
    }

    public interface ITextHttp : IHttp
    {
        Task<ApiResult<string>> Request(HttpMethod method, Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<ApiResult<string>> Get(Uri uri, Option<HttpRequestOption> config);
        Task<ApiResult<string>> Post(Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<ApiResult<string>> Put(Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<ApiResult<string>> Delete(Uri uri, Option<HttpRequestOption> config);

        Task<string> NRequest(HttpMethod method, Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<string> NGet(Uri uri, Option<HttpRequestOption> config);
        Task<string> NPost(Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<string> NPut(Uri uri, Option<string> data, Option<HttpRequestOption> config);
        Task<string> NDelete(Uri uri, Option<HttpRequestOption> config);
    }
    /// <summary>
    /// Text HTTP is a HTTP requester that specially works with string as input and output.
    /// </summary>
    public class TextHttp : ITextHttp, IDisposable
    {
        const string HttpErrorCodePrefix = "http-";
        static readonly MediaTypeHeaderValue JsonMimeType = new MediaTypeHeaderValue("application/json");

        readonly HttpClient http = new HttpClient();

        public string NetworkErrorCode => "network-issue";

        #region IDisposable methods

        public void Dispose() {
            http.Dispose();
        }

        #endregion

        public Option<HttpStatusCode> ParseHttpError(string httpErrorCode) => httpErrorCode.StartsWith(HttpErrorCodePrefix)? (HttpStatusCode) int.Parse(httpErrorCode.Substring(HttpErrorCodePrefix.Length)) : None<HttpStatusCode>();

        public Task<ApiResult<string>> Request(HttpMethod method
                                              , Uri uri
                                              , Option<string> data
                                              , Option<HttpRequestOption> config) =>
            ApiResult<string>.SafeCallAsync(() => NRequest(method, uri, data, config));

        public async Task<string> NRequest( HttpMethod method
                                                    , Uri uri
                                                    , Option<string> data
                                                    , Option<HttpRequestOption> config) {
            var req = new HttpRequestMessage(method, uri);
            data.Then(s =>
            {
                req.Content = new StringContent(s);
                req.Content.Headers.ContentType = JsonMimeType;
            });
            config.Then(ApplyConfig(req));

            HttpResponseMessage res;
            string text;
            try {
                res = await http.SendAsync(req);
                text = await res.Content.ReadAsStringAsync();
            }
            catch (Exception ex) {
                throw ExceptionExtension.ChainError("HTTP invocation failed.", NetworkErrorCode, $"{nameof(TextHttp)}:{nameof(Request)}", uri.ToString())(ex);
            }

            return res.IsSuccessStatusCode
                       ? text
                       : throw ExceptionExtension.CreateError(res.ReasonPhrase, $"{HttpErrorCodePrefix}{(int) res.StatusCode}", uri.ToString(), text);
        }

        public Task<string> NGet(Uri uri, Option<HttpRequestOption> config) => NRequest(HttpMethod.Get, uri, None<string>(), config);
        public Task<string> NPost(Uri uri, Option<string> data, Option<HttpRequestOption> config) => NRequest(HttpMethod.Post, uri, data, config);
        public Task<string> NPut(Uri uri, Option<string> data, Option<HttpRequestOption> config) => NRequest(HttpMethod.Put, uri, data, config);
        public Task<string> NDelete(Uri uri, Option<HttpRequestOption> config) => NRequest(HttpMethod.Delete, uri, null!, config);

        public Task<ApiResult<string>> Get(Uri uri, Option<HttpRequestOption> config) => Request(HttpMethod.Get, uri, null!, config);
        public Task<ApiResult<string>> Post(Uri uri, Option<string> data, Option<HttpRequestOption> config) => Request(HttpMethod.Post, uri, data, config);
        public Task<ApiResult<string>> Put(Uri uri, Option<string> data, Option<HttpRequestOption> config) => Request(HttpMethod.Put, uri, data, config);
        public Task<ApiResult<string>> Delete(Uri uri, Option<HttpRequestOption> config) => Request(HttpMethod.Delete, uri, null!, config);

        static Action<HttpRequestOption> ApplyConfig(HttpRequestMessage req) => config => {
            config.Authentication.Then(auth =>
                req.Headers.Authorization = auth.Parameter.Get( p => new AuthenticationHeaderValue(auth.Scheme, p)
                                                              , () => new AuthenticationHeaderValue(auth.Scheme)));
            config.CustomHeaders.Then(headers => headers.ForEach(kv => req.Headers.Add(kv.Key, kv.Value)));
        };

    }
}
