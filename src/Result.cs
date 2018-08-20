﻿using System;
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

        public static ApiResult<(A, B)> With<A, B>(ApiResult<A> a, ApiResult<B> b) => a.Chain(ax => b.Map(bx => (ax, bx)));
        public static ApiResult<(A, B, C)> With<A, B, C>(ApiResult<A> a, ApiResult<B> b, ApiResult<C> c) =>
            a.Chain(ax => b.Chain(bx => c.Map(cx => (ax, bx,cx))));

        public static ApiResult<T> Call<A, B, T>(this ApiResult<(A, B)> x, Func<A, B, T> f) => x.Map(p => p.CallFrom(f));
        public static ApiResult<T> Call<A, B, C, T>(this ApiResult<(A, B, C)> x, Func<A, B, C, T> f) => x.Map(p => p.CallFrom(f));
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
            error = default(TFail);
            data = success;
        }
        public Result(TFail fail)
        {
            isFailed = true;
            data = default(TSuccess);
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
            data = default(T);
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

        public static async Task<ApiResult<T>> SafeCallAsync1(Func<Task<ApiResult<T>>> f) {
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
        public Exception GetFail() => IsFail? error : throw new InvalidOperationException();

        public ApiResult<T> Where(Func<T, bool> predicate, Exception failed) => IsSuccess && predicate(data) ? this : failed;

        public U Get<U>(Func<T, U> success, Func<Exception, U> fail) => IsFail? fail(error) : success(data);

        public ApiResult<U> Map<U>(Func<T, U> mapper) => IsFail? new ApiResult<U>(error) : mapper(data);
        public ApiResult<U> Map<U>(Func<T, U> mapper, Func<Exception,Exception> failMapper) =>
            IsFail? new ApiResult<U>(failMapper(error)) : mapper(data);
        public ApiResult<U> Chain<U>(Func<T, ApiResult<U>> mapper) => IsFail? new ApiResult<U>(error) : mapper(data);

        public Task<ApiResult<U>> ChainAsync<U>(Func<T, Task<ApiResult<U>>> mapper) {
            return IsSuccess ? mapper(data) : Task.FromResult(error.AsApiFailure<U>());
        }
        public ApiResult<T> OrTry(Func<ApiResult<T>> tryFunc) => IsFail ? tryFunc() : this;

        public Task<ApiResult<T>> OrTryAsync(Func<Task<ApiResult<T>>> tryFunc) => IsFail ? tryFunc() : Task.FromResult(this);

        public ApiResult<T> Apply(Action<T> f)
        {
            if (IsSuccess) f(data);
            return this;
        }
        public ApiResult<T> IfFail(Action<Exception> f)
        {
            if (IsFail) f(error);
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
                fFail(error);
            return this;
        }
    }
}
