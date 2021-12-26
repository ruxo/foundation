using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RZ.Foundation.Functional.TaskOption
{
    public readonly struct TaskOptionNone<A>
    {
        public static readonly TaskOptionNone<A> Value = new();

        public Awaiter GetAwaiter() => new();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly struct Awaiter : INotifyCompletion
        {
            public bool IsCompleted => false;
            public A GetResult() => default!;
            public void OnCompleted(Action continuation) => Task.Run(continuation);
        }
    }
}