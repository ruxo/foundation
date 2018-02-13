using System;

namespace RZ.Foundation {
    public static class Prelude {
        public static Func<T> Constant<T>(T x) => () => x;
        public static T Identity<T>(T x) => x;
        public static void Noop() { }

        public static Func<T, T> SideEffect<T>(Action<T> f) => x => {
            f(x);
            return x;
        };
    }

    public struct Unit
    {
        public static readonly Unit Value = new Unit();
        public override bool Equals(object obj) => obj is Unit;
        public override int GetHashCode() => 0;
    }
}
