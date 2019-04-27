using System;
using LanguageExt;

namespace RZ.Foundation.Extensions
{
    public static class TupleExtension
    {
        public static T CallFrom<A, B, T>(this (A, B) t, Func<A, B, T> f) => f(t.Item1, t.Item2);
        public static T CallFrom<A, B, C, T>(this (A, B, C) t, Func<A, B, C, T> f) => f(t.Item1, t.Item2, t.Item3);

        public static Unit CallFrom<A, B>(this (A, B) t, Action<A, B> f) {
            f(t.Item1, t.Item2);
            return Unit.Default;
        }

        public static Unit CallFrom<A, B, C>(this (A, B, C) t, Action<A, B, C> f) {
            f(t.Item1, t.Item2, t.Item3);
            return Unit.Default;
        }
    }
}