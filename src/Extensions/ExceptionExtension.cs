using System;

namespace RZ.Foundation.Extensions
{
    public static class ExceptionExtension
    {
        const string CodeField = "code";
        const string DataField = "data";
        public static Exception CreateError(string message, string code, string source, object data)
        {
            var exn = new Exception(message) { Source = source };
            exn.Data.Add(CodeField, code);
            exn.Data.Add(DataField, data);
            return exn;
        }
        public static Option<string> GetErrorCode(this Exception exn) => exn.GetData(CodeField).TryCast<string>();
        public static Option<object> GetData(this Exception exn) => exn.GetData(DataField);
        public static Option<object> GetData(this Exception exn, string field) => exn.Data.Contains(field) ? exn.Data[field] : null;
    }
}
