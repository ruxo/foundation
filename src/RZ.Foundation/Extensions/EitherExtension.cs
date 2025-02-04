using System;
using System.Threading.Tasks;

// ReSharper disable UnusedType.Global

namespace RZ.Foundation.Extensions;

public static class EitherExtension
{
    public static R GetRight<L, R>(this Either<L, R> either) => either.Match(r => r, _ => throw new InvalidOperationException("Either is in Left state"));
    public static L GetLeft<L, R>(this Either<L, R> either) => either.Match(_ => throw new InvalidOperationException("Either is in Right state"), l => l);

    public static Option<L> Left<L, R>(this Either<L, R> either) => either.Match(_ => None, Some);
    public static Option<R> Right<L, R>(this Either<L, R> either) => either.Match(Some, _ => None);
    
    public static bool IfLeft<L, R>(this Either<L, R> either, out L error, out R data) {
        error = either.IsLeft ? either.GetLeft() : default!;
        data = either.IsRight ? either.GetRight() : default!;
        return either.IsLeft;
    }
    
    public static bool IfRight<L, R>(this Either<L, R> either, out R data, out L error) {
        error = either.IsLeft ? either.GetLeft() : default!;
        data = either.IsRight ? either.GetRight() : default!;
        return either.IsRight;
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