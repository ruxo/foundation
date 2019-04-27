using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
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

        public static Task<Either<T, Exception>> MapEither<T>(this Task<T> task) => MapEither(task, CancellationToken.None);

        public static Task<Either<T, Exception>> MapEither<T>(this Task<T> task, CancellationToken token) =>
            task.ContinueWith(t => token.IsCancellationRequested || t.IsCanceled || t.IsFaulted
                                 ? GetException((Task) t).AsFailure<T, Exception>()
                                 : t.Result.AsSuccess<T, Exception>()
                             , token
                             , TaskContinuationOptions.ExecuteSynchronously
                             , TaskScheduler.Current
                             );

        #region API Result helpers

        public static Task<Result<T>> ToApiResult<T>(this Task<T> task) => ToApiResult(task, CancellationToken.None);

        public static bool IsSuccess(this Task task) =>
        #if NET47 || NET471 || NET472
            task.IsCompleted && !(task.IsCanceled || task.IsFaulted);
        #else
            task.IsCompletedSuccessfully;
        #endif

        public static Task<Result<T>> ToApiResult<T>(this Task<T> task, CancellationToken token) =>
            task.ContinueWith(t => !token.IsCancellationRequested && t.IsSuccess()
                                 ? t.Result
                                 : GetException(t).AsApiFailure<T>()
                             , token
                             , TaskContinuationOptions.ExecuteSynchronously
                             , TaskScheduler.Current
                             );

        public static Task<Result<T>> TrapTaskErrors<T>(this Task<Result<T>> task) {
            return task.ContinueWith(t => t.IsSuccess() ? t.Result : GetException((Task) t).AsApiFailure<T>());
        }

        public static Task<Result<Unit>> ToApiResult(this Task task) => ToApiResult(task, CancellationToken.None);

        public static Task<Result<Unit>> ToApiResult(this Task task, CancellationToken token) =>
            task.ContinueWith(t => !token.IsCancellationRequested && t.IsSuccess()
                                 ? Unit.Default
                                 : GetException(t).AsApiFailure<Unit>()
                             , token
                             , TaskContinuationOptions.ExecuteSynchronously
                             , TaskScheduler.Current
                             );

        public static Task<Result<U>> MapResult<T, U>(this Task<Result<T>> result, Func<T, U> mapper) =>
            result.Map(r => r.Map(mapper));

        public static Task<Result<TB>> ChainResult<TA, TB>(this Task<Result<TA>> task, Func<TA, Task<Result<TB>>> chain)
        {
            var result = new TaskCompletionSource<Result<TB>>();
            task.Then(x => {
                          if (x.IsFaulted)
                              result.SetResult(faulted<TB>(x.GetFaulted().UnwrapAggregateException()));
                          else
                              TaskToCompletion(result, chain(x.Get()));
                      }
                    , result.SetException
                    , result.SetCanceled);
            return result.Task;
        }

        public static Task<Result<B>> ChainResult<A, B>(this Task<Result<A>> task, Func<A, Result<B>> chain) =>
            task.Map(r => r.Bind(chain));

        public static Task<Result<T>> OrElseResult<T>( this Task<Result<T>> task
                                                        , Func<Exception, Task<Result<T>>> orElse) {
            var result = new TaskCompletionSource<Result<T>>();

            task.Then(x => {
                          if (x.IsFaulted)
                              TaskToCompletion(result, orElse(x.GetFaulted().UnwrapAggregateException()));
                          else
                              result.SetResult(x.Get());
                      }
                    , result.SetException
                    , result.SetCanceled);
            return result.Task;
        }

        public static Exception UnwrapAggregateException(this Exception ex) => ex is AggregateException ae ? ae.InnerException : ex;

        public static Task<Result<T>> OrElseResult<T>(this Task<Result<T>> task
                                                       , Func<Exception, Result<T>> orElse) =>
            OrElseResult(task, ex => Task.FromResult(orElse(ex)));

        public static Task<Result<T>> OrElseResult<T>(this Task<Result<T>> task, Result<T> orElse) =>
            OrElseResult(task, _ => Task.FromResult(orElse));

        public static Task<U> GetResult<T,U>(this Task<Result<T>> result, Func<T,U> success, Func<Exception,U> failure) =>
            result.Map(r => r.Match(success, failure));

        public static void WorkWithResult<T>(this Task<Result<T>> task, Action<Result<T>> handler) =>
            task.Then(handler, ex => handler(faulted<T>(ex)), () => handler(faulted<T>(task.Exception)));

        public static void WorkWithResult<T>(this Task<Result<T>> task, Action<T> successHandler, Action<Exception> failHandler) =>
            task.Then(result => result.Then(successHandler, failHandler), failHandler, () => failHandler(task.Exception));

        public static void WorkWithSuccessResult<T>(this Task<Result<T>> task, Action<T> handler) => task.Then(r => r.IfSucc(handler));

        public static Task<Result<T[]>> JoinResults<T>(this Task<Result<T>[]> results) =>
            Map(results,
                        rs => {
                var failures = rs.Where(r => r.IsFaulted).AsArray();
                return failures.Length == 0
                           ? rs.Select(r => r.Get()).AsArray().AsApiSuccess()
                           : faulted<T[]>(new AggregateException(failures.Select(r => r.GetFaulted().UnwrapAggregateException())));
            });

        public static Task<Result<(A,B)>> JoinResults<A,B>(Task<Result<A>> taskA, Task<Result<B>> taskB) {
            var result = new TaskCompletionSource<Result<(A,B)>>();
            var tasks = Task.WhenAll(taskA, taskB);
            tasks.Then(() => {
                if (!taskA.IsSuccess() )
                    result.SetException(taskA.Exception);
                else if (!taskB.IsSuccess())
                    result.SetException(taskB.Exception);
                else {
                    var a = taskA.Result;
                    var b = taskB.Result;
                    result.SetResult(ResultHelper.With(a, b));
                }
            }, result.SetException, result.SetCanceled);
            return result.Task;
        }

        public static Task<Result<B>> CastResult<A, B>(this Task<Result<A>> result) where A : B =>
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
                {
                    Debug.Assert(t.IsCompleted);
                    chain(t.Result)
                        .Then(success: r => result.SetResult(r),
                            faulted: ex => result.SetException(ex),
                            canceled: () => result.SetCanceled());
                }
            });
            return result.Task;
        }

        static void TaskToCompletion<T>(TaskCompletionSource<Result<T>> result, Task<Result<T>> task) {
            task.Then(result.SetResult, result.SetException, result.SetCanceled);
        }

        public static Task Then<T>(this Task<T> task, Action<T> success = null, Action<Exception> faulted = null, Action canceled = null)
        {
            return task.ContinueWith(t => {
                if (t.IsSuccess())
                    success?.Invoke(t.Result);
                else if (t.IsFaulted)
                    faulted?.Invoke(t.Exception);
                else
                {
                    Contract.Assert(t.IsCanceled);
                    canceled?.Invoke();
                }
            });
        }
        public static Task Then(this Task task, Action success = null, Action<Exception> faulted = null, Action canceled = null)
        {
            return task.ContinueWith(t => {
                if (t.IsSuccess()) success?.Invoke();
                else if (t.IsFaulted) faulted?.Invoke(t.Exception);
                else
                {
                    Contract.Assert(t.IsCanceled);
                    canceled?.Invoke();
                }
            });
        }
    }
}