using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LanguageExt;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Functional.TaskOption
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct TaskOptionAwaiter<A> : INotifyCompletion
    {
        readonly TaskAwaiter<(bool IsSome, A Value)> awaiter;

        internal TaskOptionAwaiter(TaskOption<A> ma) => awaiter = ma.Data.GetAwaiter();

        public bool IsCompleted => awaiter.IsCompleted;

        public void OnCompleted(Action completion) => awaiter.OnCompleted(completion);

        public Option<A> GetResult() {
            var (isSome, value) = awaiter.GetResult();
            return isSome ? value : None;
        }
    }
}