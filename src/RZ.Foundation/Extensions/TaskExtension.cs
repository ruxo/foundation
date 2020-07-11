using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using static RZ.Foundation.Prelude;
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RZ.Foundation.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<Unit> AsUnitTask(this Task task) {
            await task;
            return Unit.Default;
        }

        /// <summary>
        /// Prevent locking from Synchronization Context
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ConfiguredTaskAwaitable NoSync(this Task t) => t.ConfigureAwait(continueOnCapturedContext: false);
        public static ConfiguredTaskAwaitable<T> NoSync<T>(this Task<T> t) => t.ConfigureAwait(continueOnCapturedContext: false);

        public static async Task<TB> Map<TA, TB>(this Task<TA> task, Func<TA, TB> mapper) => mapper(await task);

        public static async Task<T> Map<T>(this Task task, Func<T> mapper) {
            await task;
            return mapper();
        }

        [Obsolete("Use Try pattern instead")]
        public static Task<Result<T>> MapEither<T>(this Task<T> task) => MapEither(task, CancellationToken.None);

        [Obsolete("Use Try pattern instead")]
        public static Task<Result<T>> MapEither<T>(this Task<T> task, CancellationToken token) =>
            task.ContinueWith(t => token.IsCancellationRequested || t.IsCanceled || t.IsFaulted
                                 ? GetException(t).AsFailure<T>()
                                 : t.Result.AsSuccess()
                             , token
                             , TaskContinuationOptions.ExecuteSynchronously
                             , TaskScheduler.Current
                             );

        #region API Result helpers

        public static bool IsSuccess(this Task task) => task.IsCompleted && !(task.IsCanceled || task.IsFaulted);

        [Obsolete("Use AsSuccess instead")]
        public static async Task<Result<T>> ToApiResult<T>(this Task<T> task) => (await task).AsSuccess();

        [Obsolete("Use TryAsync instead")]
        public static Task<Result<T>> TrapTaskErrors<T>(this Task<Result<T>> task) {
            return task.ContinueWith(t => t.IsSuccess() ? t.Result : GetException((Task) t).AsFailure<T>());
        }

        public static Exception UnwrapAggregateException(this Exception ex) => ex is AggregateException ae ? ae.InnerException : ex;

        [Obsolete("Use GetOrElse instead")]
        public static Task<U> GetResult<T,U>(this Task<Result<T>> result, Func<T,U> success, Func<Exception,U> failure) =>
            result.Map(r => r.Match(success, failure));

        [Obsolete("Use Then instead")]
        public static void WorkWithResult<T>(this Task<Result<T>> task, Action<Result<T>> handler) =>
            task.Then(handler, ex => handler(new Result<T>(ex)), () => handler(new Result<T>(task.Exception)));

        [Obsolete("Use Then instead")]
        public static void WorkWithResult<T>(this Task<Result<T>> task, Action<T> successHandler, Action<Exception> failHandler) =>
            task.Then(result => result.Then(successHandler, failHandler), failHandler, () => failHandler(task.Exception));

        [Obsolete("Use Then instead")]
        public static void WorkWithSuccessResult<T>(this Task<Result<T>> task, Action<T> handler) => task.Then(r => r.Then(handler));

        [Obsolete]
        public static Task<Result<T[]>> JoinResults<T>(this Task<Result<T>[]> results) =>
            results.Map(rs => {
                var failures = rs.Where(r => r.IsFaulted).AsArray();
                return failures.Length == 0
                           ? rs.Select(r => r.GetSuccess()).AsArray().AsSuccess()
                           : new Result<T[]>(new AggregateException(failures.Select(r => r.GetFail().UnwrapAggregateException())));
            });

        [Obsolete]
        public static Task<Result<(A,B)>> JoinResults<A,B>(Task<Result<A>> taskA, Task<Result<B>> taskB) {
            var result = new TaskCompletionSource<Result<(A,B)>>();
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

        [Obsolete("Use TryCast instead")]
        public static Task<Result<B>> CastResult<A, B>(this Task<Result<A>> result) where A : B =>
            result.Map(x => (B) x);

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