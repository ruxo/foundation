using System;
using System.Threading.Tasks;
using LanguageExt;

namespace RZ.Foundation.Extensions
{
    public static class EitherExtension
    {
        public static R GetRight<L, R>(this Either<L, R> either) => either.Match(r => r, _ => throw new InvalidOperationException("Either is in Left state"));
        public static L GetLeft<L, R>(this Either<L, R> either) => either.Match(_ => throw new InvalidOperationException("Either is in Right state"), l => l);

        public static async Task<Either<L, R1>> MapT<L, R, R1>(this Either<L, R> either, Func<R, Task<R1>> f) =>
            either.IsRight ? await f(either.GetRight()) : either.GetLeft();
        public static async ValueTask<Either<L, R1>> MapT<L, R, R1>(this Either<L, R> either, Func<R, ValueTask<R1>> f) =>
            either.IsRight ? await f(either.GetRight()) : either.GetLeft();

        public static async Task<Either<L, R1>> BindT<L, R, R1>(this Either<L, R> either, Func<R, Task<Either<L, R1>>> f) =>
            either.IsRight ? await f(either.GetRight()) : either.GetLeft();
        public static async ValueTask<Either<L, R1>> BindT<L, R, R1>(this Either<L, R> either, Func<R, ValueTask<Either<L, R1>>> f) =>
            either.IsRight ? await f(either.GetRight()) : either.GetLeft();
    }
}