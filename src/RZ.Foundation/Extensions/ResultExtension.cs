using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Extensions;

public static class ResultExtension
{
    /// <summary>
    /// Helper function for creating a success result from any value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Result<T> AsSuccess<T>(this T data) => data;

    public static async Task<Result<T>> AsSuccess<T>(this Task<T> data) => (await data).AsSuccess();

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

    #region GetOrElse & GetOrThrow

    public static T GetOrElse<T>(this Result<T> result, T valForError) => result.Match(identity, _ => valForError);
    public static T GetOrElse<T>(this Result<T> result, Func<Exception, T> errorMap) => result.Match(identity, errorMap);

    public static async Task<T> GetOrElseAsync<T>(this Result<T> result, Func<Exception, Task<T>> errorMap) =>
        await result.Match(Task.FromResult, errorMap);

    public static T GetOrThrow<T>(this Result<T> result) => result.Match(identity, ex => throw new Exception("Result is error.", ex));

    public static async Task<Result<T>> GetOrElse<T>(this Task<Result<T>> result, T valForError) => (await result).GetOrElse(valForError);
    public static async Task<Result<T>> GetOrElse<T>(this Task<Result<T>> result, Func<Exception, T> errorMap) => (await result).GetOrElse(errorMap);

    public static async Task<Result<T>> GetOrElse<T>(this Task<Result<T>> result, Func<Exception, Task<T>> errorMap) =>
        await (await result).Match(Task.FromResult, errorMap);

    public static async Task<Result<T>> GetOrThrow<T>(this Task<Result<T>> result) => (await result).GetOrThrow();

    #endregion

    public static Option<Exception> Fail<T>(this Result<T> either) => either.Match(_ => None, Some);
    public static Option<T> Success<T>(this Result<T> either) => either.Match(Some, _ => None);
    
    public static bool IfFaulted<T>(this Result<T> either, out Exception error, out T data) {
        error = either.IsFaulted ? either.GetFail() : default!;
        data = either.IsSuccess ? either.GetSuccess() : default!;
        return either.IsFaulted;
    }
    
    public static bool IfSuccess<T>(this Result<T> either, out T data, out Exception error) {
        error = either.IsFaulted ? either.GetFail() : default!;
        data = either.IsSuccess ? either.GetSuccess() : default!;
        return either.IsSuccess;
    }

    public static async Task<Result<U>> Map<T, U>(this Task<Result<T>> result, Func<T, U> mapper) =>
        (await result).Map(mapper);

    public static async Task<Result<U>> Map<T, U>(this Task<Result<T>> result, Func<T, Task<U>> mapper) =>
        await (await result).MapAsync(mapper);

    #region Bind

    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> mapper) =>
        result.Match(mapper, ex => new Result<U>(ex));

    public static async Task<Result<U>> BindAsync<T, U>(this Result<T> result, Func<T, Task<Result<U>>> mapper) =>
        result.IsSuccess
            ? await mapper(result.GetSuccess())
            : new Result<U>(result.GetFail());

    public static async Task<Result<U>> Bind<T, U>(this Task<Result<T>> resultTask, Func<T, Result<U>> mapper) =>
        (await resultTask).Bind(mapper);

    public static async Task<Result<U>> Bind<T, U>(this Task<Result<T>> resultTask, Func<T, Task<Result<U>>> mapper) =>
        await (await resultTask).BindAsync(mapper);

    #endregion

    public static Result<T> Join<T>(this Result<Result<T>> results) => results.GetOrElse(AsFailure<T>);

    #region OrElse

    public static Result<T> OrElse<T>(this Result<T> result, Result<T> anotherResult) =>result.IsSuccess ? result : anotherResult;
    public static Result<T> OrElse<T>(this Result<T> result, T val) => result.OrElse(new Result<T>(val));
    public static Result<T> OrElse<T>(this Result<T> result, Func<Result<T>> tryFunc) => result.OrElse(tryFunc());

    public static async Task<Result<T>> OrElseAsync<T>(this Result<T> result, Func<Task<T>> tryFunc) => result.OrElse(await tryFunc());
    public static async Task<Result<T>> OrElseAsync<T>(this Result<T> result, Func<Task<Result<T>>> tryFunc) => result.OrElse(await tryFunc());

    public static async Task<Result<T>> OrElse<T>(this Task<Result<T>> result, Result<T> anotherResult) => (await result).OrElse(anotherResult);
    public static async Task<Result<T>> OrElse<T>(this Task<Result<T>> result, T val) => (await result).OrElse(val);
    public static async Task<Result<T>> OrElse<T>(this Task<Result<T>> result, Func<Result<T>> tryFunc) => (await result).OrElse(tryFunc());

    public static async Task<Result<T>> OrElse<T>(this Task<Result<T>> result, Func<Task<T>> tryFunc) => (await result).OrElse(await tryFunc());
    public static async Task<Result<T>> OrElse<T>(this Task<Result<T>> result, Func<Task<Result<T>>> tryFunc) => (await result).OrElse(await tryFunc());

    #endregion

    public static async Task<Result<T>> IfFailAsync<T>(this Result<T> result, Func<Exception,Task> f) {
        if (result.IsFaulted)
            await f(result.GetFail());
        return result;
    }

    public static async Task<Result<T>> IfFail<T>(this Task<Result<T>> resultTask, Action<Exception> f) {
        var result = await resultTask;
        if (result.IsFaulted)
            f(result.GetFail());
        return result;
    }

    public static async Task<Result<T>> IfFail<T>(this Task<Result<T>> resultTask, Func<Exception,Task> f)
    {
        var result = await resultTask;
        if (result.IsFaulted)
            await f(result.GetFail());
        return result;
    }

    #region Then

    public static async Task<Result<T>> Then<T>(this Task<Result<T>> resultTask, Action<T> f) => (await resultTask).Then(f);

    public static async Task<Result<T>> Then<T>(this Task<Result<T>> resultTask, Action<T> fSuccess, Action<Exception> fFail) =>
        (await resultTask).Then(fSuccess, fFail);

    public static async Task<Result<T>> Then<T>(this Task<Result<T>> resultTask, Func<T,Task> fSuccess) =>
        await (await resultTask).ThenAsync(fSuccess);

    public static async Task<Result<T>> Then<T>(this Task<Result<T>> resultTask, Func<T,Task> fSuccess, Func<Exception, Task> fFail) =>
        await (await resultTask).ThenAsync(fSuccess, fFail);


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

    #endregion

    public static Result<B> TryCast<A, B>(this Result<A> result) where A: class =>
        result.Match(x => x is B v? v : new InvalidCastException().AsFailure<B>() , AsFailure<B>);

    public static async Task<Result<B>> TryCast<A, B>(this Task<Result<A>> result) where A: class =>
        (await result).Match(x => x is B v? v : new InvalidCastException().AsFailure<B>() , AsFailure<B>);

    public static Result<T> Where<T>(this Result<T> result, Predicate<T> predicate, Func<Exception> failed) =>
        result.IsSuccess && predicate(result.GetSuccess()) ? result : failed().AsFailure<T>();

    public static Result<R> Call<A, B, R>(this Result<(A, B)> x, Func<A, B, R> f) => x.Map(p => p.CallFrom(f));
    public static Result<R> Call<A, B, C, R>(this Result<(A, B, C)> x, Func<A, B, C, R> f) => x.Map(p => p.CallFrom(f));
}