using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Extensions
{
    public static class ResultExtension
    {
        /// <summary>
        /// Helper function for creating a success result from any value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Result<T> AsSuccess<T>(this T data) => data;

        /// <summary>
        /// Helper function for creating a failure result from any value.
        /// </summary>
        /// <typeparam name="T">type parameter for success case</typeparam>
        /// <param name="ex">an exception</param>
        /// <returns>A result value represents a failure.</returns>
        public static Result<T> AsFailure<T>(this Exception ex) => new Result<T>(ex);

        /// <summary>
        /// Get instance of success type.
        /// </summary>
        /// <returns>Instance of success type.</returns>
        public static T GetSuccess<T>(this Result<T> result) =>
            result.Match(identity, ex => throw new InvalidOperationException($"Result {result} is not success.", ex));

        /// <summary>
        /// Get instance of failed type.
        /// </summary>
        /// <returns>Instance of failed type.</returns>
        public static Exception GetFail<T>(this Result<T> result) =>
            result.Match(_ => throw new InvalidOperationException($"Result {result} is not a failure."), identity);

        public static Result<U> Chain<T, U>(this Result<T> result, Func<T, Result<U>> mapper) =>
            result.Match(mapper, ex => new Result<U>(ex));

        public static async Task<Result<U>> ChainAsync<T, U>(this Result<T> result, Func<T, Task<Result<U>>> mapper) =>
            result.IsSuccess
                ? await mapper(result.GetSuccess())
                : new Result<U>(result.GetFail());

        /// <summary>
        /// Call <paramref name="f"/> if current result is success.
        /// </summary>
        /// <param name="result">A result value</param>
        /// <param name="f"></param>
        /// <returns>Always return current result.</returns>
        public static Result<T> Then<T>(this Result<T> result, Action<T> f) {
            result.IfSucc(f);
            return result;
        }
        public static Result<T> Then<T>(this Result<T> result, Action<T> fSuccess, Action<Exception> fFail) {
            result.IfSucc(fSuccess);
            result.IfFail(fFail);
            return result;
        }
        public static async Task<Result<T>> ThenAsync<T>(this Result<T> result, Func<T,Task> fSuccess) {
            if (result.IsSuccess)
                await fSuccess(result.GetSuccess());
            return result;
        }
        public static async Task<Result<T>> ThenAsync<T>(this Result<T> result, Func<T,Task> fSuccess, Func<Exception, Task> fFail)
        {
            if (result.IsSuccess)
                await fSuccess(result.GetSuccess());
            else
                await fFail(result.GetFail());
            return result;
        }
    }
}