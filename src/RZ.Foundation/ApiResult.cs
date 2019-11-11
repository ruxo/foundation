using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RZ.Foundation.Extensions;

namespace RZ.Foundation
{
    /// <summary>
    /// Helper functions for <see cref="Result{TSuccess, TFail}"/>
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
        public static Result<T, TFail> AsSuccess<T, TFail>(this T data) => data;
        /// <summary>
        /// Helper function for creating a failure result from any value.
        /// </summary>
        /// <typeparam name="TSuccess"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Result<TSuccess, T> AsFailure<TSuccess, T>(this T data) => data;

        public static ApiResult<T> AsApiSuccess<T>(this T data) => data;
        public static ApiResult<T> AsApiFailure<T>(this Exception data) => data;

        public static Task<ApiResult<T>> AsTaskApiSuccess<T>(this T data) => Task.FromResult((ApiResult<T>)data);
        public static Task<ApiResult<T>> AsTaskApiFailure<T>(this Exception ex) => Task.FromResult((ApiResult<T>) ex);

        public static Task<ApiResult<U>> ChainApiTask<T, U>(this ApiResult<T> result, Func<T, Task<ApiResult<U>>> f) => result.Get(f, ex => Task.FromResult((ApiResult<U>) ex));
        public static Task<ApiResult<U>> ChainApiTask<T, U>(this ApiResult<T> result, Func<T, Task<U>> f) =>
            result.Get(x => ApiResult<U>.SafeCallAsync(() => f(x)), ex => Task.FromResult((ApiResult<U>) ex));

        public static ApiResult<T> AsApiResult<T>(this Task<ApiResult<T>> t) => t.IsSuccess() ? t.Result : t.Exception;

        public static T GetOrThrow<T>(this ApiResult<T> result) =>
            result.IsSuccess
                ? result.GetSuccess()
                : throw ExceptionExtension.ChainError("Unhandled exception"
                                                    , "unhandled"
                                                    , "ApiResult")(result.GetFail());

        #if NETSTANDARD2_2
        [Obsolete("Use Prelude")]
        public static ApiResult<(A, B)> With<A, B>(ApiResult<A> a, ApiResult<B> b) => a.Chain(ax => b.Map(bx => (ax, bx)));
        [Obsolete("Use Prelude")]
        public static ApiResult<(A, B, C)> With<A, B, C>(ApiResult<A> a, ApiResult<B> b, ApiResult<C> c) =>
            a.Chain(ax => b.Chain(bx => c.Map(cx => (ax, bx,cx))));

        public static ApiResult<T> Call<A, B, T>(this ApiResult<(A, B)> x, Func<A, B, T> f) => x.Map(p => p.CallFrom(f));
        public static ApiResult<T> Call<A, B, C, T>(this ApiResult<(A, B, C)> x, Func<A, B, C, T> f) => x.Map(p => p.CallFrom(f));
        #endif
    }

    /// <summary>
    /// Represent two-state result.
    /// </summary>
    /// <typeparam name="TSuccess">Type that represents success data.</typeparam>
    /// <typeparam name="TFail">Type that represents failed data.</typeparam>
    public struct Result<TSuccess, TFail>
    {
        readonly bool isFailed;
        readonly TFail error;
        readonly TSuccess data;

        public Result(TSuccess success)
        {
            isFailed = false;
            error = default;
            data = success;
        }
        public Result(TFail fail)
        {
            isFailed = true;
            data = default;
            error = fail;
        }

        /// <summary>
        /// Check if this result represents success state.
        /// </summary>
        public bool IsSuccess => !isFailed;
        /// <summary>
        /// Check if this result represents failure state.
        /// </summary>
        public bool IsFail => isFailed;
        /// <summary>
        /// Get instance of success type.
        /// </summary>
        /// <returns>Instance of success type.</returns>
        public TSuccess GetSuccess() => isFailed ? throw new InvalidOperationException() : data;
        /// <summary>
        /// Get instance of failed type.
        /// </summary>
        /// <returns>Instance of failed type.</returns>
        public TFail GetFail() => isFailed? error : throw new InvalidOperationException();

        public Result<TSuccess, TFail> Where(Func<TSuccess, bool> predicate, TFail failed) => IsSuccess && predicate(data) ? this : failed;

        /// <summary>
        /// Transform Result into a value.
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="success">Transformer function for success case.</param>
        /// <param name="fail">Transformer function for failure case.</param>
        /// <returns></returns>
        public T Get<T>(Func<TSuccess, T> success, Func<TFail, T> fail) => isFailed? fail(error) : success(data);
        /// <summary>
        /// Transform success result to other success type. Do nothing if current result is a failure.
        /// </summary>
        /// <typeparam name="U">Target success type</typeparam>
        /// <param name="mapper">Function to transform success type.</param>
        /// <returns>New Result type</returns>
        public Result<U, TFail> Map<U>(Func<TSuccess, U> mapper) => isFailed? (Result<U,TFail>) error : mapper(data);
        /// <summary>
        /// Transform success result to other success or failed type. Do nothing if current result is a failure.
        /// </summary>
        /// <typeparam name="U">Target success type</typeparam>
        /// <param name="mapper">Function to transform success type to either U type or _TFail_ type. </param>
        /// <returns></returns>
        public Result<U, TFail> Chain<U>(Func<TSuccess, Result<U, TFail>> mapper) => isFailed? (Result<U,TFail>) error : mapper(data);
        /// <summary>
        /// Convert Result.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="successMapper"></param>
        /// <param name="failMapper"></param>
        /// <returns></returns>
        public Result<U, V> MapAll<U, V>(Func<TSuccess, U> successMapper, Func<TFail, V> failMapper) => isFailed? (Result<U,V>) failMapper(error) : successMapper(data);

        /// <summary>
        /// Try calling <paramref name="f"/> if current result is failure.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public Result<TSuccess, TFail> OrElse(Func<TFail, Result<TSuccess, TFail>> f) => isFailed? f(error) : this;

        [Obsolete("Use Then instead")]
        public Result<TSuccess, TFail> Apply(Action<TSuccess> f) => Then(f);

        /// <summary>
        /// Call <paramref name="f"/> if current result is success.
        /// </summary>
        /// <param name="f"></param>
        /// <returns>Always return current result.</returns>
        public Result<TSuccess, TFail> Then(Action<TSuccess> f)
        {
            if (!isFailed) f(data);
            return this;
        }

        public static implicit operator Result<TSuccess, TFail>(TSuccess success) => new Result<TSuccess, TFail>(success);
        public static implicit operator Result<TSuccess, TFail>(TFail failed) => new Result<TSuccess, TFail>(failed);
    }

    /// <summary>
    /// Represent two-state result. Failure state is represented by <seealso cref="Exception"/>
    /// </summary>
    /// <typeparam name="T">Type that represents success data.</typeparam>
    public struct ApiResult<T>
    {
        readonly Exception error;
        readonly T data;

        public ApiResult(T success)
        {
            error = null;
            data = success;
        }
        public ApiResult(Exception fail)
        {
            data = default;
            error = fail;
        }

        public static implicit operator ApiResult<T>(T success) => Equals(success,null)? new InvalidOperationException() : new ApiResult<T>(success);
        public static implicit operator ApiResult<T>(Exception failed) => failed == null? new ArgumentNullException(nameof(failed)) : new ApiResult<T>(failed);
        public static ApiResult<T> SafeCall(Func<T> f) {
            try {
                return f();
            }
            catch (Exception ex) {
                return ex;
            }
        }

        public static async Task<ApiResult<T>> SafeCallAsync(Func<Task<T>> f) {
            try {
                return await f();
            }
            catch (Exception ex) {
                return ex;
            }
        }

        public static async Task<ApiResult<T>> SafeCallAsync(Func<Task<ApiResult<T>>> f) {
            try {
                return await f();
            }
            catch (Exception ex) {
                return ex;
            }
        }

        public bool IsSuccess => error == null;
        public bool IsFail => error != null;
        public T GetSuccess() => IsFail? throw new InvalidOperationException() : data;
        public Exception GetFail() => IsFail? error! : throw new InvalidOperationException();

        public T GetOrElse(T valForError) => IsSuccess ? data : valForError;
        public T GetOrElse(Func<Exception, T> errorMap) => IsSuccess ? data : errorMap(error!);
        public async Task<T> GetOrElseAsync(Func<Exception, Task<T>> errorMap) => IsSuccess ? data : await errorMap(error!);
        public T GetOrThrow() => IsSuccess ? data : throw new Exception("ApiResult is error.", error);

        public ApiResult<T> Where(Func<T, bool> predicate, Exception failed) => IsSuccess && predicate(data) ? this : failed;
        public async Task<ApiResult<T>> WhereAsync(Func<T, Task<bool>> predicate, Exception failed) => IsSuccess && await predicate(data) ? this : failed;

        public ApiResult<U> Map<U>(Func<T, U> mapper) => IsSuccess? mapper(data) : new ApiResult<U>(error!);
        public ApiResult<U> Map<U>(Func<T, U> mapper, Func<Exception,Exception> failMapper) => IsSuccess? mapper(data) : new ApiResult<U>(failMapper(error!));

        public async Task<ApiResult<U>> MapAsync<U>(Func<T, Task<U>> mapper) => IsSuccess? await mapper(data) : new ApiResult<U>(error!);
        public async Task<ApiResult<U>> MapAsync<U>(Func<T, Task<U>> mapper, Func<Exception,Task<Exception>> failMapper) =>
            IsSuccess? await mapper(data) : new ApiResult<U>(await failMapper(error!));

        public ApiResult<U> Chain<U>(Func<T, ApiResult<U>> mapper) => IsFail? new ApiResult<U>(error!) : mapper(data);

        public async Task<ApiResult<U>> ChainAsync<U>(Func<T, Task<ApiResult<U>>> mapper) => IsSuccess ? await mapper(data) : error!.AsApiFailure<U>();

        public ApiResult<T> OrElse(T val) => IsSuccess ? this : val.AsApiSuccess();
        public ApiResult<T> OrElse(ApiResult<T> anotherResult) => IsSuccess ? this : anotherResult;
        public ApiResult<T> OrElse(Func<ApiResult<T>> tryFunc) => IsSuccess ? this : tryFunc();

        public async Task<ApiResult<T>> OrElseAsync(Func<Task<ApiResult<T>>> tryFunc) => IsSuccess ? this : await tryFunc();

        public U Get<U>(Func<T, U> success, Func<Exception, U> fail) => IsFail? fail(error!) : success(data);

        [Obsolete("Use OrElse instead")]
        public ApiResult<T> OrTry(Func<ApiResult<T>> tryFunc) => IsFail ? tryFunc() : this;

        [Obsolete("Use OrElseAsync instead")]
        public Task<ApiResult<T>> OrTryAsync(Func<Task<ApiResult<T>>> tryFunc) => IsFail ? tryFunc() : Task.FromResult(this);

        [Obsolete("Use Then instead")]
        public ApiResult<T> Apply(Action<T> f)
        {
            if (IsSuccess) f(data);
            return this;
        }
        public ApiResult<T> IfFail(Action<Exception> f)
        {
            if (IsFail) f(error!);
            return this;
        }
        public async Task<ApiResult<T>> IfFailAsync(Func<Exception,Task> f)
        {
            if (IsFail) await f(error!);
            return this;
        }
        public ApiResult<T> Then(Action<T> fSuccess)
        {
            if (IsSuccess) fSuccess(data);
            return this;
        }
        public ApiResult<T> Then(Action<T> fSuccess, Action<Exception> fFail)
        {
            if (IsSuccess)
                fSuccess(data);
            else
                fFail(error!);
            return this;
        }
        public async Task<ApiResult<T>> ThenAsync(Func<T,Task> fSuccess)
        {
            if (IsSuccess) await fSuccess(data);
            return this;
        }
        public async Task<ApiResult<T>> ThenAsync(Func<T, Task> fSuccess, Func<Exception, Task> fFail)
        {
            if (IsSuccess)
                await fSuccess(data);
            else
                await fFail(error!);
            return this;
        }

        public ApiResult<U> TryCast<U>() => IsSuccess? (U) (object) data : error!.AsApiFailure<U>();

        #region Equality

        public override bool Equals(object obj) => obj is ApiResult<T> other && Equals(other);

        public bool Equals(ApiResult<T> other) =>
            (other.IsFail && IsFail) ||
            (other.IsSuccess && IsSuccess && EqualityComparer<T>.Default.Equals(other.data, data));

        public override int GetHashCode() => IsSuccess? EqualityComparer<T>.Default.GetHashCode(data) : error!.GetHashCode();

        #endregion
    }
}
