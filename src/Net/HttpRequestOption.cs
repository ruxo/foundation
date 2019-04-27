using System.Collections.Generic;
using LanguageExt;

namespace RZ.Foundation.Net
{
    public struct HttpAuthentication
    {
        public string Scheme;
        public Option<string> Parameter;
        public static HttpAuthentication Bearer(string parameter) => new HttpAuthentication { Scheme = "Bearer", Parameter = parameter };
    }
    public struct HttpRequestOption
    {
        public Option<HttpAuthentication> Authentication;
        public Option<Dictionary<string, string>> CustomHeaders;
    }
}
