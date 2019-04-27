using System;
using System.Runtime.InteropServices;
using LanguageExt;
using RZ.Foundation.Extensions;
using static LanguageExt.Prelude;

namespace RZ.Foundation
{
    public static class OptionHelper
    {
        static readonly Exception DummyException = new Exception();

        public static T Get<T>(this Option<T> opt) =>
            opt.IfNone(() => throw ExceptionExtension.CreateError("Unhandled exception", "unhandled", "Option", opt));

        public static Either<T, F> ToResult<T, F>(this Option<T> o, Func<F> none) => o.IsSome ? o.Get().AsSuccess<T,F>() : none();

        public static Result<T> ToApiResult<T>(this Option<T> o) =>
            o.Match(some => new Result<T>(some), () => new Result<T>(DummyException));

        public static Result<T> ToApiResult<T>(this Option<T> o, Func<Exception> none) =>
            o.Match(some => new Result<T>(some), () => new Result<T>(none()));

        public static Option<(A, B)> With<A, B>(Option<A> a, Option<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));
        public static Option<(A, B, C)> With<A, B, C>(Option<A> a, Option<B> b, Option<C> c) =>
            a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx,cx))));

        public static Option<T> Call<A, B, T>(this Option<(A, B)> x, Func<A, B, T> f) => x.Map(p => p.CallFrom(f));
        public static Option<T> Call<A, B, C, T>(this Option<(A, B, C)> x, Func<A, B, C, T> f) => x.Map(p => p.CallFrom(f));

        public static Option<Unit> Call<A, B>(this Option<(A, B)> x, Action<A, B> f) => x.Map(p => p.CallFrom(f));
        public static Option<Unit> Call<A, B, C>(this Option<(A, B, C)> x, Action<A, B, C> f) => x.Map(p => p.CallFrom(f));
    }

    [StructLayout(LayoutKind.Auto)]
    public struct OptionSerializable<T>
    {
        public bool HasValue;
        public T Value;
        public OptionSerializable(Option<T> opt)
        {
            HasValue = opt.IsSome;
            Value = opt.Match(identity, () => default);
        }
        public Option<T> ToOption() => HasValue ? Some(Value) : None;
    }
}
