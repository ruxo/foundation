// ReSharper disable UnusedType.Global

namespace RZ.Foundation.Extensions;

public static class EitherExtension
{
    extension<L, R>(Either<L, R> either)
    {
        public R         GetRight() => either.Match(r => r, _ => throw new InvalidOperationException("Either is in Left state"));
        public L         GetLeft()  => either.Match(_ => throw new InvalidOperationException("Either is in Right state"), l => l);
        public Option<L> Left()     => either.Match(_ => None, Some);
        public Option<R> Right()    => either.Match(Some, _ => None);

        public bool IfLeft(out L error, out R data) {
            error = either.IsLeft ? either.GetLeft() : default!;
            data = either.IsRight ? either.GetRight() : default!;
            return either.IsLeft;
        }

        public bool IfRight(out R data, out L error) {
            error = either.IsLeft ? either.GetLeft() : default!;
            data = either.IsRight ? either.GetRight() : default!;
            return either.IsRight;
        }
    }

    public static async Task<Either<L, R1>> Map<L, R, R1>(this Task<Either<L, R>> task, Func<R, R1> f) => (await task).Map(f);
    public static async ValueTask<Either<L, R1>> Map<L, R, R1>(this ValueTask<Either<L, R>> task, Func<R, R1> f) => (await task).Map(f);

    public static async Task<Either<L, R1>> Map<L, R, R1>(this Task<Either<L, R>> task, Func<R, Task<R1>> f) => await (await task).MapT(f);
    public static async ValueTask<Either<L, R1>> Map<L, R, R1>(this ValueTask<Either<L, R>> task, Func<R, ValueTask<R1>> f) => await (await task).MapTV(f);

    public static async Task<Either<L, R1>> TryMap<L, R, R1>(this Task<Either<L, R>> task, Func<R, R1> f, Func<Exception, L> left) {
        try {
            var v = await task;
            return v.Map(f);
        }
        catch (Exception e) {
            return left(e);
        }
    }
    public static async Task<Either<L, R1>> TryMap<L, R, R1>(this Task<Either<L, R>> task, Func<R, Task<R1>> f, Func<Exception,L> left) {
        var v = await task;
        try {
            return await v.MapT(f);
        }
        catch (Exception e) {
            return left(e);
        }
    }

    public static async Task<Either<L, R1>> MapT<L, R, R1>(this Either<L, R> either, Func<R, Task<R1>> f) =>
        either.IsRight ? await f(either.GetRight()) : either.GetLeft();
    public static async ValueTask<Either<L, R1>> MapTV<L, R, R1>(this Either<L, R> either, Func<R, ValueTask<R1>> f) =>
        either.IsRight ? await f(either.GetRight()) : either.GetLeft();

    public static async Task<Either<L, R1>> Bind<L, R, R1>(this Task<Either<L, R>> task, Func<R, Either<L,R1>> f) => (await task).Bind(f);
    public static async Task<Either<L, R1>> Bind<L, R, R1>(this Task<Either<L, R>> task, Func<R, Task<Either<L,R1>>> f) => await (await task).BindT(f);
    public static async ValueTask<Either<L, R1>> Bind<L, R, R1>(this ValueTask<Either<L, R>> task, Func<R, ValueTask<Either<L,R1>>> f) => await (await task).BindTV(f);

    public static async Task<Either<L, R1>> BindT<L, R, R1>(this Either<L, R> either, Func<R, Task<Either<L, R1>>> f) =>
        either.IsRight ? await f(either.GetRight()) : either.GetLeft();
    public static async ValueTask<Either<L, R1>> BindTV<L, R, R1>(this Either<L, R> either, Func<R, ValueTask<Either<L, R1>>> f) =>
        either.IsRight ? await f(either.GetRight()) : either.GetLeft();

    public static async Task<Either<L, R1>> TryBind<L, R, R1>(this Task<Either<L, R>> task, Func<R, Either<L,R1>> f, Func<Exception, L> left) {
        try {
            var v = await task;
            return v.Bind(f);
        }
        catch (Exception e) {
            return left(e);
        }
    }
    public static async Task<Either<L, R1>> TryBind<L, R, R1>(this Task<Either<L, R>> task, Func<R, Task<Either<L,R1>>> f, Func<Exception,L> left) {
        var v = await task;
        try {
            return await v.BindT(f);
        }
        catch (Exception e) {
            return left(e);
        }
    }

}