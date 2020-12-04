using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;

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

        #region API Result helpers

        public static bool IsSuccess(this Task task) => task.IsCompleted && !(task.IsCanceled || task.IsFaulted);

        public static Exception UnwrapAggregateException(this Exception ex) => ex is AggregateException ae ? ae.InnerException! : ex;

        #endregion

        public static Task<TB> Chain<TA, TB>(this Task<TA> task, Func<TA, Task<TB>> chain)
        {
            var result = new TaskCompletionSource<TB>();
            task.ContinueWith(t => {
                if (t.IsCanceled)
                    result.SetCanceled();
                else if (t.IsFaulted)
                    // ReSharper disable once AssignNullToNotNullAttribute
                    result.SetException(t.Exception!);
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
                    faulted?.Invoke(t.Exception!);
                else
                    canceled?.Invoke();
            });

        public static Task Then(this Task task, Action? success = null, Action<Exception>? faulted = null, Action? canceled = null) =>
            task.ContinueWith(t => {
                if (t.IsSuccess()) success?.Invoke();
                else if (t.IsFaulted) faulted?.Invoke(t.Exception!);
                else
                    canceled?.Invoke();
            });
    }
}