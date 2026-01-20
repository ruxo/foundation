using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMethodReturnValue.Global

namespace RZ.Foundation.Extensions;

[PublicAPI]
public static class OptionHelper
{
    extension<T>(Option<T> o)
    {
        [PublicAPI]
        public bool IfSome([NotNullWhen(true)] out T? data) {
            data = o.IsSome? (T)o.Case! : default;
            return o.IsSome;
        }

        [PublicAPI]
        public bool UnlessSome([NotNullWhen(false)] out T? data) {
            data = o.IsSome? (T)o.Case! : default;
            return o.IsNone;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<ImmutableHashSet<T>> ToNotEmptyOption<T>(this ImmutableHashSet<T> data) =>
        data.IsEmpty ? None : data;

    public static Option<T[]> ToNotEmptyOption<T>(this IEnumerable<T> data) {
        var array = data.AsArray();
        return array.Length > 0 ? array : None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Option<T> Join<T>(this Option<Option<T>> doubleOption) => doubleOption.Bind(i => i);

    extension<A, B>(Option<(A, B)> x)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> Call<T>(Func<A, B, T> f) => x.Map(p => p.CallFrom(f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<Unit> Call(Action<A, B> f) => x.Map(p => p.CallFrom(f));
    }

    extension<A, B, C>(Option<(A, B, C)> x)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> Call<T>(Func<A, B, C, T> f) => x.Map(p => p.CallFrom(f));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<Unit> Call(Action<A, B, C> f) => x.Map(p => p.CallFrom(f));
    }

    extension<T>(Task<Option<T>> t)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Option<TR>> Chain<TR>(Func<T, Option<TR>> mapper) => (await t).Bind(mapper);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<Option<TR>> ChainAsync<TR>(Func<T, Task<Option<TR>>> mapper) =>
            await (await t).ToAsync().BindAsync(async x => (await mapper(x)).ToAsync()).ToOption();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> Get() => (await t).Get();

        [Obsolete("use Match instead")]
        public Task<TR> Get<TR>(Func<T, TR> someMapper, Func<TR> noneMapper) => t.Match(someMapper, noneMapper);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> Match<TR>(Func<T, TR> someMapper, Func<TR> noneMapper) =>
            (await t).Match(someMapper, noneMapper);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<TR> MatchAsync<TR>(Func<T, Task<TR>> someMapper, Func<Task<TR>> noneMapper) =>
            await (await t).MatchAsync(someMapper, noneMapper);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> GetOrElse(T noneValue) => (await t).IfNone(noneValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> GetOrElse(Func<T> noneValue) => (await t).IfNone(noneValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<T> GetOrElseAsync(Func<Task<T>> noneValue) => await (await t).IfNoneAsync(noneValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? ToNullable<T>(this Option<T> opt) where T : class => opt.IfNoneUnsafe((T?)null);

    /// <param name="opt">current option</param>
    /// <typeparam name="T">type parameter for opt</typeparam>
    extension<T>(Option<T> opt)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get() => opt.GetOrThrow(() => new InvalidOperationException("Option value is in None state"));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public B Get<B>(Func<T,B> getter) => opt.IsSome? getter(opt.Get()) : throw new InvalidOperationException("Option value is in None state");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetOrThrow(Func<Exception> noneHandler) => opt.IfNone(() => throw noneHandler());

        public async Task<T> GetOrThrowT(Func<Task<Exception>> noneHandler) {
            if (opt.IsNone)
                throw await noneHandler();
            return opt.Get();
        }

        public async ValueTask<T> GetOrThrowT(Func<ValueTask<Exception>> noneHandler) {
            if (opt.IsNone)
                throw await noneHandler();
            return opt.Get();
        }

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="elseValue">Value to replace if none</param>
        /// <returns>Result option type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> OrElse(T elseValue) => opt.IsSome? opt : Some(elseValue);

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="elseValue">Value to replace if none</param>
        /// <returns>Result option type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> OrElse(Option<T> elseValue) => opt.IsSome? opt : elseValue;

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="elseFunc">A replace function to evaluate if none</param>
        /// <returns>Result option type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<T> OrElse(Func<Option<T>> elseFunc) => opt.IsSome? opt : elseFunc();

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="elseFunc">A replace function to evaluate if none</param>
        /// <returns>Result option type</returns>
        public ValueTask<Option<T>> OrElseAsync(Func<ValueTask<Option<T>>> elseFunc) =>
            opt.IsSome ? new ValueTask<Option<T>>(opt) : elseFunc();

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="elseFunc">A replace function to evaluate if none</param>
        /// <returns>Result option type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<Option<T>> OrElseT(Func<Task<Option<T>>> elseFunc) =>
            opt.IsSome ? Task.FromResult(opt) : elseFunc();

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="elseFunc">A replace function to evaluate if none</param>
        /// <returns>Result option type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<Option<T>> OrElseT(Func<ValueTask<Option<T>>> elseFunc) =>
            opt.IsSome ? ValueTask.FromResult(opt) : elseFunc();

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="elseFunc">A replace function to evaluate if none</param>
        /// <returns>Result option type</returns>
        [Obsolete("Use OrElseT")]
        public Task<Option<T>> OrElseAsync(Func<Task<Option<T>>> elseFunc) =>
            opt.IsSome ? Task.FromResult(opt) : elseFunc();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Option<R> TryCast<R>() =>
            opt.IsSome && opt.Get() is R v ? Some(v) : None;
    }
}

[PublicAPI, StructLayout(LayoutKind.Auto)]
public readonly struct OptionSerializable<T>(Option<T> opt)
{
    public readonly bool HasValue = opt.IsSome;
    public readonly T? Value = opt.IfNoneUnsafe((T?)default);
    public Option<T> ToOption() => HasValue ? Some(Value!) : None;
}