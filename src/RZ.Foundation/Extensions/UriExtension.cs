using System;
using System.Diagnostics;

namespace RZ.Foundation.Extensions
{
    public static class UriExtension
    {
        public static Uri Combine(this Uri @base, params string[] paths) {
            var (hostAndPath, query, fragment) = SplitQueryFragment(@base.AbsoluteUri);
            var host = EnsureEndsWith('/', hostAndPath);
            return new($"{host}{paths.Join('/')}{PrefixIfExisted('?', query)}{PrefixIfExisted('#', fragment)}");
        }

        static string EnsureEndsWith(char c, string s) => s.EndsWith(c) ? s : $"{s}{c}";

        static string PrefixIfExisted(char c, Option<string> opt) => opt.Map(s => $"{c}{s}").IfNone(string.Empty);

        static (string HostPart, Option<string> Path, Option<string> QueryAndFragment) SplitQueryFragment(string uri) {
            var queryIndex = FindChar('?', uri);
            var fragmentIndex = FindChar('#', uri);
            Debug.Assert(fragmentIndex.IsNone || fragmentIndex >= queryIndex);
            return (queryIndex.IsSome, fragmentIndex.IsSome) switch
            {
                (false, false) => (uri, None, None),
                (false, true) => (uri.Substring(0, fragmentIndex.Get()), None, fragmentIndex.Map(fi => uri.Substring(fi + 1))),
                (true, false) => (uri.Substring(0, queryIndex.Get()), queryIndex.Map(qi => uri.Substring(qi + 1)), None),
                (true, true) => (uri.Substring(0, queryIndex.Get()), queryIndex.Map(qi => uri.Substring(qi + 1, fragmentIndex.Get() - qi)),
                                 fragmentIndex.Map(fi => uri.Substring(fi + 1)))
            };
        }

        static Option<int> FindChar(char c, string s) {
            var i = s.IndexOf(c);
            return i == -1 ? None : Some(i);
        }
    }
}