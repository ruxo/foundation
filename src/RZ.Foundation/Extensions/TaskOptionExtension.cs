using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RZ.Foundation.Types;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace RZ.Foundation.Extensions;

[PublicAPI]
public static class TaskOptionExtension
{
    #region Then

    extension<A>(Option<A> option)
    {
        [PublicAPI, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async ValueTask<Option<A>> IterAsync(Func<A, ValueTask> sideEffect) {
            if (option.IsSome) await sideEffect(option.Get());
            return option;
        }

        [PublicAPI]
        public Option<A> Iter(Action<A> handler) {
            option.IfSome(handler);
            return option;
        }

        [PublicAPI]
        public Option<A> Iter(Action<A> someHandler, Action noneHandler) {
            option.Match(someHandler, noneHandler);
            return option;
        }

        [PublicAPI]
        public async ValueTask<Option<A>> IterAsync(Func<A, ValueTask> someHandler, Func<ValueTask> noneHandler) {
            if (option.IsSome) await someHandler(option.Get());
            else await noneHandler();
            return option;
        }
    }

    extension<A>(ValueTask<Option<A>> option)
    {
        [PublicAPI]
        public async ValueTask<Option<A>> Iter(Action<A> sideEffect) => Iter(await option, sideEffect);

        [PublicAPI]
        public async ValueTask<Option<A>> Iter(Func<A, ValueTask> sideEffect) => await (await option).IterAsync(sideEffect);
    }

    #endregion

    #region Map

    public static async ValueTask<Option<B>> MapAsync<A, B>(this Option<A> option, Func<A, ValueTask<B>> map) => option.IsSome ? await map(option.Get()) : None;

    extension<A>(ValueTask<Option<A>> option)
    {
        [PublicAPI]
        public async ValueTask<Option<B>> MapAsync<B>(Func<A, ValueTask<B>> map) => await (await option).MapAsync(map);

        [PublicAPI]
        public async ValueTask<Option<B>> Map<B>(Func<A, B> mapper) => (await option).Map(mapper);
    }

    #endregion

    [PublicAPI, Pure]
    public static async ValueTask<bool> IfSome<T>(this ValueTask<Option<T>> opt, MutableRef<T> data) {
        var o = await opt;
        data.Value = o.IsSome ? o.Get() : default!;
        return o.IsSome;
    }

    [PublicAPI]
    public static async ValueTask<B> Get<A, B>(this Option<A> opt, Func<A, ValueTask<B>> getter) => opt.IsSome ? await getter(opt.Get()) : throw new InvalidOperationException("Option value is in None state");

    [PublicAPI]
    public static async ValueTask<B?> GetOrDefault<A, B>(this Option<A> opt, Func<A, ValueTask<B>> getter) {
        var result = await opt.MapAsync(getter);
        return result.IsSome ? result.Get() : default;
    }

    extension<T>(ValueTask<Option<T>> opt)
    {
        [PublicAPI]
        public async ValueTask<T> GetOrThrow(Func<Exception> noneHandler) => (await opt).GetOrThrow(noneHandler);

        [PublicAPI]
        public async ValueTask<T> GetOrThrow(Func<ValueTask<Exception>> noneHandler) => await (await opt).GetOrThrowT(noneHandler);

        [PublicAPI, Pure]
        public async ValueTask<Option<T>> OrElse(Option<T> noneValue) => (await opt).OrElse(noneValue);

        [PublicAPI]
        public async ValueTask<Option<T>> OrElse(Func<Option<T>> noneHandler) => (await opt).OrElse(noneHandler);

        [PublicAPI]
        public async ValueTask<Option<T>> OrElse(Func<ValueTask<Option<T>>> noneHandler) {
            var result = await opt;
            return result.IsSome ? result : await noneHandler();
        }

        [PublicAPI]
        public async ValueTask<T> IfNone(T noneValue) => (await opt).IfNone(noneValue);

        [PublicAPI]
        public async ValueTask<T> IfNone(Func<T> noneHandler) => (await opt).IfNone(noneHandler);
    }
}