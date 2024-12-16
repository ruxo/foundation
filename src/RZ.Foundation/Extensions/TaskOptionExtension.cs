using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using RZ.Foundation.Types;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace RZ.Foundation.Extensions;

public static class TaskOptionExtension
{
    #region Then

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<Option<A>> ThenT<A>(this Option<A> option, Func<A, Task> sideEffect) {
        await option.IfSomeAsync(sideEffect);
        return option;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask<Option<A>> ThenT<A>(this Option<A> option, Func<A, ValueTask> sideEffect) {
        if (option.IsSome)
            await sideEffect(option.Get());
        return option;
    }

    public static async Task<Option<A>> Then<A>(this Task<Option<A>> option, Action<A> sideEffect) => (await option).Then(sideEffect);
    public static async Task<Option<A>> Then<A>(this Task<Option<A>> option, Func<A, ValueTask> sideEffect) => await (await option).ThenT(sideEffect);
    public static async Task<Option<A>> Then<A>(this Task<Option<A>> option, Func<A, Task> sideEffect) => await (await option).ThenT(sideEffect);
    public static async ValueTask<Option<A>> Then<A>(this ValueTask<Option<A>> option, Action<A> sideEffect) => (await option).Then(sideEffect);
    public static async ValueTask<Option<A>> Then<A>(this ValueTask<Option<A>> option, Func<A, ValueTask> sideEffect) => await (await option).ThenT(sideEffect);
    public static async Task<Option<A>> Then<A>(this ValueTask<Option<A>> option, Func<A, Task> sideEffect) => await (await option).ThenT(sideEffect);

    public static Option<T> Then<T>(this Option<T> opt, Action<T> handler) {
        opt.IfSome(handler);
        return opt;
    }

    public static Option<T> Then<T>(this Option<T> opt, Action<T> someHandler, Action noneHandler) {
        opt.Match(someHandler, noneHandler);
        return opt;
    }

    public static async Task<Option<T>> ThenAsync<T>(this Option<T> opt, Func<T, Task> handler) {
        await opt.IfSomeAsync(handler);
        return opt;
    }

    public static async Task<Option<T>> ThenAsync<T>(this Option<T> opt, Func<T, Task> someHandler, Func<Task> noneHandler) {
        if (opt.IsSome) await opt.IfSomeAsync(someHandler);
        else await noneHandler();
        return opt;
    }
    #endregion

    #region Map

    public static async Task<Option<B>>      MapT<A, B>(this Option<A> option, Func<A, Task<B>> map)       => option.IsSome ? await map(option.Get()) : None;
    public static async ValueTask<Option<B>> MapTV<A, B>(this Option<A> option, Func<A, ValueTask<B>> map) => option.IsSome ? await map(option.Get()) : None;

    public static async Task<Option<B>> MapT<A, B>(this Task<Option<A>> option, Func<A, Task<B>> map) => await (await option).MapT(map);
    public static async ValueTask<Option<B>> MapT<A, B>(this ValueTask<Option<A>> option, Func<A, ValueTask<B>> map) => await (await option).MapTV(map);

    public static async Task<Option<B>> Map<A, B>(this Task<Option<A>> t, Func<A, B> mapper) => (await t).Map(mapper);
    public static async ValueTask<Option<B>> Map<A, B>(this ValueTask<Option<A>> t, Func<A, B> mapper) => (await t).Map(mapper);

    [Obsolete("Use MapT instead")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Option<B>> MapAsync<A, B>(this Task<Option<A>> t, Func<A, Task<B>> mapper) => t.MapT(mapper);

    #endregion

    public static async ValueTask<bool> IfSome<T>(this ValueTask<Option<T>> opt, MutableRef<T> data) {
        var o = await opt;
        data.Value = o.IsSome ? o.Get() : default!;
        return o.IsSome;
    }

    public static async Task<B> GetT<A,B>(this Option<A> opt, Func<A,Task<B>> getter) => opt.IsSome? await getter(opt.Get()) : throw new InvalidOperationException();
    public static async ValueTask<B> GetTV<A,B>(this Option<A> opt, Func<A,ValueTask<B>> getter) => opt.IsSome? await getter(opt.Get()) : throw new InvalidOperationException();

    public static Task<B?> GetOrDefaultT<A, B>(this Option<A> opt, Func<A, Task<B>> getter) => opt.MapT(getter).IfNoneUnsafeAsync(default(B));
    public static async ValueTask<B?> GetOrDefaultTV<A, B>(this Option<A> opt, Func<A, ValueTask<B>> getter) {
        var result = await opt.MapTV(getter);
        return result.IsSome ? result.Get() : default;
    }

    public static async Task<T> GetOrThrow<T>(this Task<Option<T>> opt, Func<Exception> noneHandler) => (await opt).GetOrThrow(noneHandler);
    public static async ValueTask<T> GetOrThrowV<T>(this ValueTask<Option<T>> opt, Func<Exception> noneHandler) => (await opt).GetOrThrow(noneHandler);

    public static async Task<T> GetOrThrow<T>(this Task<Option<T>> opt, Func<Task<Exception>> noneHandler) => await (await opt).GetOrThrowT(noneHandler);
    public static async Task<T> GetOrThrow<T>(this Task<Option<T>> opt, Func<ValueTask<Exception>> noneHandler) => await (await opt).GetOrThrowT(noneHandler);
    public static async ValueTask<T> GetOrThrow<T>(this ValueTask<Option<T>> opt, Func<ValueTask<Exception>> noneHandler) => await (await opt).GetOrThrowT(noneHandler);
    public static async Task<T> GetOrThrow<T>(this ValueTask<Option<T>> opt, Func<Task<Exception>> noneHandler) => await (await opt).GetOrThrowT(noneHandler);

    public static async Task<Option<T>> OrElse<T>(this Task<Option<T>> opt, Option<T> noneValue) => (await opt).OrElse(noneValue);
    public static async ValueTask<Option<T>> OrElse<T>(this ValueTask<Option<T>> opt, Option<T> noneValue) => (await opt).OrElse(noneValue);

    public static async Task<Option<T>> OrElse<T>(this Task<Option<T>> opt, Func<Option<T>> noneHandler) => (await opt).OrElse(noneHandler);
    public static async ValueTask<Option<T>> OrElse<T>(this ValueTask<Option<T>> opt, Func<Option<T>> noneHandler) => (await opt).OrElse(noneHandler);

    public static async Task<Option<T>> OrElse<T>(this Task<Option<T>> opt, Func<Task<Option<T>>> noneHandler) {
        var result = await opt;
        return result.IsSome? result : await noneHandler();
    }
    public static async Task<Option<T>> OrElse<T>(this ValueTask<Option<T>> opt, Func<Task<Option<T>>> noneHandler) {
        var result = await opt;
        return result.IsSome? result : await noneHandler();
    }
    public static async ValueTask<Option<T>> OrElse<T>(this ValueTask<Option<T>> opt, Func<ValueTask<Option<T>>> noneHandler) {
        var result = await opt;
        return result.IsSome? result : await noneHandler();
    }

    public static async Task<T> IfNone<T>(this Task<Option<T>> opt, T noneValue) => (await opt).IfNone(noneValue);
    public static async ValueTask<T> IfNone<T>(this ValueTask<Option<T>> opt, T noneValue) => (await opt).IfNone(noneValue);

    public static async Task<T> IfNone<T>(this Task<Option<T>> opt, Func<T> noneHandler) => (await opt).IfNone(noneHandler);
    public static async ValueTask<T> IfNone<T>(this ValueTask<Option<T>> opt, Func<T> noneHandler) => (await opt).IfNone(noneHandler);
}