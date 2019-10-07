using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Prevent locking from Synchronization Context
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ConfiguredTaskAwaitable NoSync(this Task t) => t.ConfigureAwait(continueOnCapturedContext: false);
        public static ConfiguredTaskAwaitable<T> NoSync<T>(this Task<T> t) => t.ConfigureAwait(continueOnCapturedContext: false);
        public static Task<TB> Map<TA, TB>(this Task<TA> task, Func<TA, TB> mapper) {
            var taskB = new TaskCompletionSource<TB>();
            task.Then(x => taskB.SetResult(mapper(x)), taskB.SetException, taskB.SetCanceled);
            return taskB.Task;
        }

        public static Task<T> Map<T>(this Task task, Func<T> mapper) {
            var taskB = new TaskCompletionSource<T>();
            task.Then(() => taskB.SetResult(mapper()), taskB.SetException, taskB.SetCanceled);
            return taskB.Task;
        }

        public static Task<Result<T, Exception>> MapEither<T>(this Task<T> task) => MapEither(task, CancellationToken.None);

        public static Task<Result<T, Exception>> MapEither<T>(this Task<T> task, CancellationToken token) =>
            task.ContinueWith(t => token.IsCancellationRequested || t.IsCanceled || t.IsFaulted
                                 ? GetException((Task) t).AsFailure<T, Exception>()
                                 : t.Result.AsSuccess<T, Exception>()
                             , token
                             , TaskContinuationOptions.ExecuteSynchronously
                             , TaskScheduler.Current
                             );

        #region API Result helpers

        public static Task<ApiResult<T>> ToApiResult<T>(this Task<T> task) => ToApiResult(task, CancellationToken.None);

        public static bool IsSuccess(this Task task) => task.IsCompleted && !(task.IsCanceled || task.IsFaulted);

        public static Task<ApiResult<T>> ToApiResult<T>(this Task<T> task, CancellationToken token) =>
            task.ContinueWith(t => !token.IsCancellationRequested && t.IsSuccess()
                                 ? t.Result
                                 : GetException(t).AsApiFailure<T>()
                             , token
                             , TaskContinuationOptions.ExecuteSynchronously
                             , TaskScheduler.Current
                             );

        public static Task<ApiResult<T>> TrapTaskErrors<T>(this Task<ApiResult<T>> task) {
            return task.ContinueWith(t => t.IsSuccess() ? t.Result : GetException((Task) t).AsApiFailure<T>());
        }

        public static Task<ApiResult<Unit>> ToApiResult(this Task task) => ToApiResult(task, CancellationToken.None);

        public static Task<ApiResult<Unit>> ToApiResult(this Task task, CancellationToken token) =>
            task.ContinueWith(t => !token.IsCancellationRequested && t.IsSuccess()
                                 ? Unit.Value
                                 : GetException(t).AsApiFailure<Unit>()
                             , token
                             , TaskContinuationOptions.ExecuteSynchronously
                             , TaskScheduler.Current
                             );

        public static Task<ApiResult<U>> MapResult<T, U>(this Task<ApiResult<T>> result, Func<T, U> mapper) =>
            result.Map(r => r.Map(mapper));

        public static Task<ApiResult<TB>> ChainResult<TA, TB>(this Task<ApiResult<TA>> task, Func<TA, Task<ApiResult<TB>>> chain)
        {
            var result = new TaskCompletionSource<ApiResult<TB>>();
            task.Then(x => {
                          if (x.IsFail)
                              result.SetResult(x.GetFail().UnwrapAggregateException());
                          else
                              TaskToCompletion(result, chain(x.GetSuccess()));
                      }
                    , result.SetException
                    , result.SetCanceled);
            return result.Task;
        }

        public static Task<ApiResult<B>> ChainResult<A, B>(this Task<ApiResult<A>> task, Func<A, ApiResult<B>> chain) =>
            task.Map(r => r.Chain(chain));

        public static Task<ApiResult<T>> OrElseResult<T>( this Task<ApiResult<T>> task
                                                        , Func<Exception, Task<ApiResult<T>>> orElse) {
            var result = new TaskCompletionSource<ApiResult<T>>();

            task.Then(x => {
                          if (x.IsFail)
                              TaskToCompletion(result, orElse(x.GetFail().UnwrapAggregateException()));
                          else
                              result.SetResult(x.GetSuccess());
                      }
                    , result.SetException
                    , result.SetCanceled);
            return result.Task;
        }

        public static Exception UnwrapAggregateException(this Exception ex) => ex is AggregateException ae ? ae.InnerException : ex;

        public static Task<ApiResult<T>> OrElseResult<T>(this Task<ApiResult<T>> task
                                                       , Func<Exception, ApiResult<T>> orElse) =>
            OrElseResult(task, ex => Task.FromResult(orElse(ex)));

        public static Task<ApiResult<T>> OrElseResult<T>(this Task<ApiResult<T>> task, ApiResult<T> orElse) =>
            OrElseResult(task, _ => Task.FromResult(orElse));

        public static Task<U> GetResult<T,U>(this Task<ApiResult<T>> result, Func<T,U> success, Func<Exception,U> failure) =>
            result.Map(r => r.Get(success, failure));

        public static void WorkWithResult<T>(this Task<ApiResult<T>> task, Action<ApiResult<T>> handler) =>
            task.Then(handler, ex => handler(ex), () => handler(task.Exception));

        public static void WorkWithResult<T>(this Task<ApiResult<T>> task, Action<T> successHandler, Action<Exception> failHandler) =>
            task.Then(result => result.Then(successHandler, failHandler), failHandler, () => failHandler(task.Exception));

        public static void WorkWithSuccessResult<T>(this Task<ApiResult<T>> task, Action<T> handler) => task.Then(r => r.Then(handler));

        public static Task<ApiResult<T[]>> JoinResults<T>(this Task<ApiResult<T>[]> results) =>
            results.Map(rs => {
                var failures = rs.Where(r => r.IsFail).AsArray();
                return failures.Length == 0
                           ? rs.Select(r => r.GetSuccess()).AsArray().AsApiSuccess()
                           : new AggregateException(failures.Select(r => r.GetFail().UnwrapAggregateException()));
            });

        public static Task<ApiResult<(A,B)>> JoinResults<A,B>(Task<ApiResult<A>> taskA, Task<ApiResult<B>> taskB) {
            var result = new TaskCompletionSource<ApiResult<(A,B)>>();
            var tasks = Task.WhenAll(taskA, taskB);
            tasks.Then(() => {
                if (!taskA.IsSuccess() )
                    result.SetException(taskA.Exception);
                else if (!taskB.IsSuccess())
                    result.SetException(taskB.Exception);
                else
                    result.SetResult(With(taskA.Result, taskB.Result));
            }, result.SetException, result.SetCanceled);
            return result.Task;
        }

        public static Task<ApiResult<B>> CastResult<A, B>(this Task<ApiResult<A>> result) where A : B =>
            result.MapResult(x => (B) x);

        #endregion

        static Exception GetException<T>(Task<T> t) => t.Exception ?? (Exception)new TaskCanceledException(t);
        static Exception GetException(Task t) => t.Exception ?? (Exception)new TaskCanceledException(t);

        public static Task<TB> Chain<TA, TB>(this Task<TA> task, Func<TA, Task<TB>> chain)
        {
            var result = new TaskCompletionSource<TB>();
            task.ContinueWith(t => {
                if (t.IsCanceled)
                    result.SetCanceled();
                else if (t.IsFaulted)
                    // ReSharper disable once AssignNullToNotNullAttribute
                    result.SetException(t.Exception);
                else
                    chain(t.Result)
                        .Then(success: r => result.SetResult(r),
                              faulted: ex => result.SetException(ex),
                              canceled: () => result.SetCanceled());
            });
            return result.Task;
        }

        static void TaskToCompletion<T>(TaskCompletionSource<ApiResult<T>> result, Task<ApiResult<T>> task) {
            task.Then(result.SetResult, result.SetException, result.SetCanceled);
        }

        public static Task Then<T>(this Task<T> task, Action<T>? success = null, Action<Exception>? faulted = null, Action? canceled = null) =>
            task.ContinueWith(t => {
                if (t.IsSuccess())
                    success?.Invoke(t.Result);
                else if (t.IsFaulted)
                    faulted?.Invoke(t.Exception);
                else
                    canceled?.Invoke();
            });

        public static Task Then(this Task task, Action? success = null, Action<Exception>? faulted = null, Action? canceled = null) =>
            task.ContinueWith(t => {
                if (t.IsSuccess()) success?.Invoke();
                else if (t.IsFaulted) faulted?.Invoke(t.Exception);
                else
                    canceled?.Invoke();
            });
    }
}