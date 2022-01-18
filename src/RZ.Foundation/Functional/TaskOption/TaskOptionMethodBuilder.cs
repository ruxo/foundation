using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RZ.Foundation.Functional.TaskOption
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct TaskOptionMethodBuilder<A>
    {
        TaskCompletionSource<(bool IsSome, A? Value)> asyncResult;
        ResultState state;

        public TaskOption<A> Task { get; private set; }

        public static TaskOptionMethodBuilder<A> Create() => new();

        public void Start<TStateMachine>(ref TStateMachine machine) where TStateMachine : IAsyncStateMachine {
            asyncResult = new();
            Task = new(asyncResult.Task);
            machine.MoveNext();
        }

        public void SetStateMachine(IAsyncStateMachine machine) { /* not sure this useless method is for.. it's never called */ }

        public void SetResult(A result) {
            switch (state) {
                case ResultState.NormalReturn:
                    asyncResult.SetResult(TaskOption<A>.MakeOptional(result));
                    state = ResultState.ResultSetComplete;
                    break;
                case ResultState.SpecialAwaiterDiscardNext:
                    state = ResultState.ResultSetComplete;
                    break;
                case ResultState.ResultSetComplete:
                    throw new InvalidOperationException("Result has already been set!");
                default:
                    throw new NotSupportedException("Invalid state!");
            }
        }

        public void SetException(Exception exception) => asyncResult.SetException(exception);

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine machine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            HandleSpecialAwaiters(ref awaiter);

            var m = machine;
            awaiter.OnCompleted(() => m.MoveNext());
        }

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine machine)
            where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine {
            HandleSpecialAwaiters(ref awaiter);

            var m = machine;
            awaiter.OnCompleted(() => m.MoveNext());
        }

        void HandleSpecialAwaiters<TAwaiter>(ref TAwaiter awaiter) {
            switch (awaiter) {
                case TaskOptionNone<A>.Awaiter:
                    state = ResultState.SpecialAwaiterDiscardNext;
                    asyncResult.SetResult((false, default!));
                    break;
                case TaskOptionFrom<A>.Awaiter returnAwaiter:
                    state = ResultState.SpecialAwaiterDiscardNext;
                    asyncResult.SetResult(returnAwaiter.Result.Match(v => (true, v), () => (false, default!)));
                    break;
            }
        }

        enum ResultState
        {
            NormalReturn = 0,
            SpecialAwaiterDiscardNext,
            ResultSetComplete,
        }
    }
}