using System;
using LanguageExt;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Extensions
{
    public static class ExceptionExtension
    {
        const string CodeField = "code";
        const string DataField = "data";
        public static Exception CreateError(string message, string code, string source, object? data = null) =>
            new Exception(message).AddMetaData( code, source, data);

        public static Func<Exception, Exception> ChainError(string message, string code, string source, object? data = null) =>
            ex => new Exception(message,ex).AddMetaData( code, source, data);
        public static Option<string> GetErrorCode(this Exception exn) => exn.GetData(CodeField).TryCast<object,string>();
        public static Option<object> GetData(this Exception exn) => exn.GetData(DataField);
        public static Option<object> GetData(this Exception exn, string field) => exn.Data.Contains(field) ? exn.Data[field].ToOption() : None;

        public static string DebugMessage(this Exception exn) =>
            exn.GetErrorCode()
               .Match(code => $"Code: {code}{Environment.NewLine}Data: {exn.GetData().Match(o => o.ToString(), () => string.Empty)}{Environment.NewLine}{exn}",
                      exn.ToString);

        public static T AddMetaData<T>(this T exn, string code, string source, object? data) where T: Exception {
            exn.Source = source;
            exn.Data.Add(CodeField, code);
            exn.Data.Add(DataField, data);
            return exn;
        }
    }
}
