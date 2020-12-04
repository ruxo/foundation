using System;
#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#endif
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Extensions
{
    public static class OptionHelper
    {
        static readonly Exception DummyException = new Exception("Dummy extension in RZ.Foundation");

        public static Option<T> ToOption<T>(this T data) => data;
        public static Option<T> ToOption<T>(this T? data) where T : struct => data.HasValue? Some(data.Value) : Option<T>.None;
        public static Option<T> None<T>() => Option<T>.None;

        public static Result<T> ToResult<T>(this Option<T> o) => o.IsSome ? o.Get().AsSuccess() : DummyException.AsFailure<T>();
        public static Result<T> ToResult<T>(this Option<T> o, Func<Exception> none) => o.IsSome ? o.Get().AsSuccess() : none().AsFailure<T>();

        public static Option<T> Join<T>(this Option<Option<T>> doubleOption) => doubleOption.Bind(i => i);

        public static Option<T> Call<A, B, T>(this Option<(A, B)> x, Func<A, B, T> f) => x.Map(p => p.CallFrom(f));
        public static Option<T> Call<A, B, C, T>(this Option<(A, B, C)> x, Func<A, B, C, T> f) => x.Map(p => p.CallFrom(f));

        public static Option<Unit> Call<A, B>(this Option<(A, B)> x, Action<A, B> f) => x.Map(p => p.CallFrom(f));
        public static Option<Unit> Call<A, B, C>(this Option<(A, B, C)> x, Action<A, B, C> f) => x.Map(p => p.CallFrom(f));

        public static async Task<Option<TR>> Map<T, TR>(this Task<Option<T>> t, Func<T, TR> mapper) => (await t).Map(mapper);
        public static async Task<Option<TR>> MapAsync<T, TR>(this Task<Option<T>> t, Func<T, Task<TR>> mapper) =>
            await (await t).ToAsync().MapAsync(mapper).ToOption();

        public static async Task<Option<TR>> Chain<T, TR>(this Task<Option<T>> t, Func<T, Option<TR>> mapper) => (await t).Bind(mapper);
        public static async Task<Option<TR>> ChainAsync<T, TR>(this Task<Option<T>> t, Func<T, Task<Option<TR>>> mapper) =>
            await (await t).ToAsync().BindAsync(async x => (await mapper(x)).ToAsync()).ToOption();

        public static async Task<T> Get<T>(this Task<Option<T>> t) => (await t).Get();
        [Obsolete("use Match instead")]
        public static Task<TR> Get<T, TR>(this Task<Option<T>> t, Func<T, TR> someMapper, Func<TR> noneMapper) => t.Match(someMapper, noneMapper);
        public static async Task<TR> Match<T, TR>(this Task<Option<T>> t, Func<T, TR> someMapper, Func<TR> noneMapper) =>
            (await t).Match(someMapper, noneMapper);

        [Obsolete("use MatchAsync instead")]
        public static Task<TR> GetAsync<T, TR>(this Task<Option<T>> t, Func<T, Task<TR>> someMapper, Func<Task<TR>> noneMapper) =>
            t.MatchAsync(someMapper, noneMapper);

        public static async Task<TR> MatchAsync<T, TR>(this Task<Option<T>> t, Func<T, Task<TR>> someMapper, Func<Task<TR>> noneMapper) =>
            await (await t).MatchAsync(someMapper, noneMapper);

        public static async Task<T> GetOrThrow<T>(this Task<Option<T>> t, Func<Exception> exceptionToThrow) => (await t).GetOrThrow(exceptionToThrow);

#if NETSTANDARD2_1
        [return: MaybeNull]
#endif
        public static async Task<T> GetOrDefault<T>(this Task<Option<T>> t) => (await t).GetOrDefault()!;

        public static async Task<T> GetOrElse<T>(this Task<Option<T>> t, T noneValue) => (await t).IfNone(noneValue);
        public static async Task<T> GetOrElse<T>(this Task<Option<T>> t, Func<T> noneValue) => (await t).IfNone(noneValue);
        public static async Task<T> GetOrElseAsync<T>(this Task<Option<T>> t, Func<Task<T>> noneValue) => await (await t).IfNoneAsync(noneValue);

        public static T? ToNullable<T>(this Option<T> opt) where T : class => opt.GetOrDefault();
    }

    public static class OptionNullableHelper
    {
        public static T? ToNullable<T>(this Option<T> opt) where T : struct => opt.GetOrDefault();
    }

    public static class OptionExtensions
    {
        public static T Get<T>(this Option<T> opt) => opt.GetOrThrow(() => new InvalidOperationException());
        public static T GetOrThrow<T>(this Option<T> opt, Func<Exception> noneHandler) => opt.IfNone(() => throw noneHandler());

        [Obsolete("use Match instead")]
        public static TResult Get<T, TResult>(this Option<T> opt, Func<T, TResult> someHandler, Func<TResult> noneHandler) =>
            opt.Match(someHandler,noneHandler);

        [Obsolete("use MatchAsync instead")]
        public static Task<TR> GetAsync<T, TR>(this Option<T> opt, Func<T, Task<TR>> someHandler, Func<Task<TR>> noneHandler) =>
            opt.MatchAsync(someHandler, noneHandler);

        #if NETSTANDARD2_1
        [return: MaybeNull]
        #endif
        public static  T GetOrDefault<T>(this Option<T> opt) => opt.IfNoneUnsafe(() => default!);

        [Obsolete("use IfNone instead")]
        public static T GetOrElse<T>(this Option<T> opt, T defaultValue) => opt.IfNone(defaultValue);
        [Obsolete("use IfNone instead")]
        public static T GetOrElse<T>(this Option<T> opt, Func<T> noneHandler) => opt.IfNone(noneHandler);
        [Obsolete("use IfNoneAsync instead")]
        public static Task<T> GetOrElseAsync<T>(this Option<T> opt, Func<Task<T>> noneHandler) => opt.IfNoneAsync(noneHandler);

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="opt">current option</param>
        /// <param name="elseValue">Value to replace if none</param>
        /// <typeparam name="T">type parameter for opt</typeparam>
        /// <returns>Result option type</returns>
        public static Option<T> OrElse<T>(this Option<T> opt, T elseValue) => opt.IsSome? opt : Some(elseValue);

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="opt">current option</param>
        /// <param name="elseValue">Value to replace if none</param>
        /// <typeparam name="T">type parameter for opt</typeparam>
        /// <returns>Result option type</returns>
        public static Option<T> OrElse<T>(this Option<T> opt, Option<T> elseValue) => opt.IsSome? opt : elseValue;

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="opt">current option</param>
        /// <param name="elseFunc">A replace function to evaluate if none</param>
        /// <typeparam name="T">type parameter for opt</typeparam>
        /// <returns>Result option type</returns>
        public static Option<T> OrElse<T>(this Option<T> opt, Func<Option<T>> elseFunc) => opt.IsSome? opt : elseFunc();

#if NETSTANDARD2_1
        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="opt">current option</param>
        /// <param name="elseFunc">A replace function to evaluate if none</param>
        /// <typeparam name="T">type parameter for opt</typeparam>
        /// <returns>Result option type</returns>
        public static ValueTask<Option<T>> OrElseAsync<T>(this Option<T> opt, Func<ValueTask<Option<T>>> elseFunc) =>
            opt.IsSome ? new ValueTask<Option<T>>(opt) : elseFunc();
#endif

        /// <summary>
        /// Replace current option value if current option is None.
        /// </summary>
        /// <param name="opt">current option</param>
        /// <param name="elseFunc">A replace function to evaluate if none</param>
        /// <typeparam name="T">type parameter for opt</typeparam>
        /// <returns>Result option type</returns>
        public static Task<Option<T>> OrElseAsync<T>(this Option<T> opt, Func<Task<Option<T>>> elseFunc) =>
            opt.IsSome ? Task.FromResult(opt) : elseFunc();

        public static Option<R> TryCast<T,R>(this Option<T> opt) => opt.Bind(value =>  value is R v? Some(v) : None);

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
    }

    [StructLayout(LayoutKind.Auto)]
    public struct OptionSerializable<T>
    {
        public bool HasValue;
#if NETSTANDARD2_1
        [AllowNull]
#endif
        public T Value;
        public OptionSerializable(Option<T> opt)
        {
            HasValue = opt.IsSome;
            Value = opt.GetOrDefault();
        }
        public Option<T> ToOption() => HasValue ? Some(Value) : None;
    }
}
