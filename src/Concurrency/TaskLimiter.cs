using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using RZ.Foundation.Extensions;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Concurrency
{
    /// <summary>
    /// Limit task concurrency. This class is built from an example from Microsoft.
    /// </summary>
    public class TaskLimiter : TaskScheduler
    {
        [ThreadStatic]
        static bool CurrentThreadIsProcessingItem;

        readonly LinkedList<Task> tasks = new LinkedList<Task>();
        int delegatesQueuedOrRunning;

        public TaskLimiter(int maxDegreeOfParallelism) {
            MaximumConcurrencyLevel = maxDegreeOfParallelism;
        }

        public static TaskFactory CreateLimitedFactory(int maxConcurrency) => new TaskFactory(new TaskLimiter(maxConcurrency));

        public override int MaximumConcurrencyLevel { get; }

        protected override IEnumerable<Task> GetScheduledTasks() {
            lock (tasks)
                return tasks.ToArray();
        }

        protected override void QueueTask(Task task) {
            lock (tasks) {
                tasks.AddLast(task);
                if (delegatesQueuedOrRunning < MaximumConcurrencyLevel) {
                    ++delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) =>
            CurrentThreadIsProcessingItem
            && !(taskWasPreviouslyQueued && task.IsCompleted)
            && TryExecuteTask(task);

        Option<Task> FetchAvailableTask() {
            lock (tasks) {
                if (tasks.Count > 0) {
                    var r = tasks.First.Value;
                    tasks.RemoveFirst();
                    return r;
                }
                else
                    return None;
            }
        }

        IEnumerable<Task> IterateAvailableTasks() {
            Option<Task> task;
            while ((task = FetchAvailableTask()).IsSome)
                yield return task.Get();
        }

        void NotifyThreadPoolOfPendingWork() {
            ThreadPool.UnsafeQueueUserWorkItem(_ => {
                CurrentThreadIsProcessingItem = true;
                try {
                    IterateAvailableTasks().ForEach(task => {
                        if (!task.IsCompleted && !TryExecuteTask(task))
                            Console.WriteLine("Execution of Task {0} failed", task.Id.ToString());
                    });
                }
                finally {
                    lock(tasks)
                        --delegatesQueuedOrRunning;
                    CurrentThreadIsProcessingItem = false;
                }
            }, null);
        }
    }
}