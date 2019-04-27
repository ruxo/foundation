using System;
using System.Threading.Tasks;
using LanguageExt;
using RZ.Foundation.Extensions;
using static RZ.Foundation.Prelude;
using static LanguageExt.Prelude;

namespace RZ.Foundation
{
    /// <summary>
    /// Helper functions for <see cref="Result{A}"/>
    /// </summary>
    public static class ResultHelper
    {
        /// <summary>
        /// Helper function for creating a success result from any value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TFail"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Either<T, TFail> AsSuccess<T, TFail>(this T data) => data;
        /// <summary>
        /// Helper function for creating a failure result from any value.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Either<TSuccess, T> AsFailure<TSuccess, T>(this T data) => data;

        public static Exception GetFaulted<T>(this Result<T> result) =>
            result.Match(_ => throw ExceptionExtension.CreateError("Unhandled exception", "unhandled", "Result", result), identity);

        public static Result<T> AsApiSuccess<T>(this T data) => data;
        public static Result<T> AsApiFailure<T>(this Exception data) => faulted<T>(data);

        public static Task<Result<T>> AsTaskApiSuccess<T>(this T data) => Task.FromResult((Result<T>)data);
        public static Task<Result<T>> AsTaskApiFailure<T>(this Exception ex) => Task.FromResult(faulted<T>(ex));

        public static Task<Result<U>> ChainApiTask<T, U>(this Result<T> result, Func<T, Task<Result<U>>> f) =>
            result.IsSuccess ? f(result.Match(identity, _ => default))
                             : Task.FromResult(faulted<U>(result.GetFaulted()));

        public static Task<Result<U>> ChainApiTask<T, U>(this Result<T> result, Func<T, Task<U>> f) => result.MapAsync(f);

        public static Result<T> AsApiResult<T>(this Task<Result<T>> t) => t.IsSuccess() ? t.Result : faulted<T>(t.Exception);

        public static T Get<T>(this Result<T> result) =>
            result.Match(
             identity,
             ex => throw ExceptionExtension.ChainError("Unhandled exception", "unhandled", "Result")(ex)
            );

        public static T GetOrDefault<T>(this Result<T> result) => result.Match(identity, _ => default);

        public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> f) => result.Match(f, faulted<U>);

        public static Result<(A, B)> With<A, B>(Result<A> a, Result<B> b) => a.Bind(ax => b.Map(bx => (ax, bx)));
        public static Result<(A, B, C)> With<A, B, C>(Result<A> a, Result<B> b, Result<C> c) =>
            a.Bind(ax => b.Bind(bx => c.Map(cx => (ax, bx,cx))));

        public static Result<T> Call<A, B, T>(this Result<(A, B)> x, Func<A, B, T> f) => x.Map(p => p.CallFrom(f));
        public static Result<T> Call<A, B, C, T>(this Result<(A, B, C)> x, Func<A, B, C, T> f) => x.Map(p => p.CallFrom(f));

        public static void Then<T>(this Result<T> result, Action<T> succHandler, Action<Exception> failHandler) {
            if (result.IsSuccess)
                result.IfSucc(succHandler);
            else
                result.IfFail(failHandler);
        }
    }
}
