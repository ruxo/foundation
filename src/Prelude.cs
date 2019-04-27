using System;
using LanguageExt;
using static LanguageExt.Prelude;
// ReSharper disable InconsistentNaming

namespace RZ.Foundation {
    public static class Prelude {
        public static Func<T> Constant<T>(T x) => () => x;
        public static void Noop() { }

        public static Func<T, T> SideEffect<T>(Action<T> f) => x => {
            f(x);
            return x;
        };

        public static Result<T> success<T>(T value) => new Result<T>(value);
        public static Result<T> faulted<T>(Exception ex) => new Result<T>(ex);
        public static Option<T> tryCast<T>(object value) {
            if (Equals(null, value)) return None;
            var converted = Convert.ChangeType(value, typeof(T));
            return Equals(converted, null)? None: Some((T)converted);
        }
    }
}
