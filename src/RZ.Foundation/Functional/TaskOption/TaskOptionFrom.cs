using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LanguageExt;

namespace RZ.Foundation.Functional.TaskOption
{
    public readonly struct TaskOptionFrom<A>
    {
        readonly Option<A> result;

        public TaskOptionFrom(Option<A> result)
        {
            this.result = result;
        }

        public Awaiter GetAwaiter() => new (result);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly struct Awaiter : INotifyCompletion
        {
            public Awaiter(Option<A> result)
            {
                Result = result;
            }

            public bool IsCompleted => false;

            internal Option<A> Result { get; }

            public A GetResult() => default!;
            public void OnCompleted(Action continuation) => Task.Run(continuation);
        }
    }
}