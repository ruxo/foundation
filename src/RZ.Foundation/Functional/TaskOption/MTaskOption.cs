using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.TypeClasses;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Functional.TaskOption
{
    public readonly struct MTaskOption<A> :
        Alternative<TaskOption<A>, Unit, A>,
        OptionalAsync<TaskOption<A>, A>,
        OptionalUnsafeAsync<TaskOption<A>, A>,
        MonadAsync<TaskOption<A>, A>,
        BiFoldableAsync<TaskOption<A>, A, Unit>
    {
        public static readonly MTaskOption<A> Inst = default;

        [Pure]
        public TaskOption<A> None => TaskOption<A>.None;

        [Pure]
        public MB Bind<MonadB, MB, B>(TaskOption<A> ma, Func<A, MB> f) where MonadB : struct, MonadAsync<Unit, Unit, MB, B> =>
            default(MonadB).RunAsync(async _ =>
                                         await ma.IsSome.ConfigureAwait(false)
                                             ? f(await ma.Value.ConfigureAwait(false))
                                             : default(MonadB).Fail(ValueIsNoneException.Default));

        [Pure]
        public MB BindAsync<MonadB, MB, B>(TaskOption<A> ma, Func<A, Task<MB>> f) where MonadB : struct, MonadAsync<Unit, Unit, MB, B> =>
            default(MonadB).RunAsync(async _ =>
                                         await ma.IsSome.ConfigureAwait(false)
                                             ? await f(await ma.Value.ConfigureAwait(false)).ConfigureAwait(false)
                                             : default(MonadB).Fail(ValueIsNoneException.Default));

        [Pure]
        public TaskOption<A> Fail(object? _ = null) => TaskOption<A>.None;

        [Pure]
        public TaskOption<A> Plus(TaskOption<A> a, TaskOption<A> b) =>
            default(MTaskOption<A>).RunAsync(async _ =>
                                                      await a.IsSome.ConfigureAwait(false)
                                                          ? a
                                                          : b);
                [Pure]
        public TaskOption<A> ReturnAsync(Func<Unit, Task<A>> f) =>
            TaskOption<A>.SomeAsync(f(unit));

        [Pure]
        public TaskOption<A> Zero() =>
            TaskOption<A>.None;

        [Pure]
        public Task<bool> IsNone(TaskOption<A> opt) =>
            opt.IsNone;

        [Pure]
        public Task<bool> IsSome(TaskOption<A> opt) =>
            opt.IsSome;

        [Pure]
        public async Task<B> Match<B>(TaskOption<A> opt, Func<A, B> Some, Func<B> None)
        {
            if (Some == null) throw new ArgumentNullException(nameof(Some));
            if (None == null) throw new ArgumentNullException(nameof(None));
            return await opt.IsSome.ConfigureAwait(false)
                ? CheckNullReturn(Some(await opt.Value.ConfigureAwait(false)))
                : CheckNullReturn(None());
        }

        [Pure]
        public async Task<B> MatchAsync<B>(TaskOption<A> opt, Func<A, Task<B>> SomeAsync, Func<B> None)
        {
            if (SomeAsync == null) throw new ArgumentNullException(nameof(SomeAsync));
            if (None == null) throw new ArgumentNullException(nameof(None));
            return await opt.IsSome.ConfigureAwait(false)
                ? CheckNullReturn(await SomeAsync(await opt.Value.ConfigureAwait(false)).ConfigureAwait(false))
                : CheckNullReturn(None());
        }

        [Pure]
        public async Task<B> MatchAsync<B>(TaskOption<A> opt, Func<A, B> Some, Func<Task<B>> NoneAsync)
        {
            if (Some == null) throw new ArgumentNullException(nameof(Some));
            if (NoneAsync == null) throw new ArgumentNullException(nameof(NoneAsync));
            return await opt.IsSome.ConfigureAwait(false)
                ? CheckNullReturn(Some(await opt.Value.ConfigureAwait(false)))
                : CheckNullReturn(await NoneAsync().ConfigureAwait(false));
        }

        [Pure]
        public async Task<B> MatchAsync<B>(TaskOption<A> opt, Func<A, Task<B>> SomeAsync, Func<Task<B>> NoneAsync)
        {
            if (SomeAsync == null) throw new ArgumentNullException(nameof(SomeAsync));
            if (NoneAsync == null) throw new ArgumentNullException(nameof(NoneAsync));
            return await opt.IsSome.ConfigureAwait(false)
                ? CheckNullReturn(await SomeAsync(await opt.Value.ConfigureAwait(false)).ConfigureAwait(false))
                : CheckNullReturn(await NoneAsync().ConfigureAwait(false));
        }

        public async Task<Unit> Match(TaskOption<A> opt, Action<A> Some, Action None)
        {
            if (Some == null) throw new ArgumentNullException(nameof(Some));
            if (None == null) throw new ArgumentNullException(nameof(None));
            if (await opt.IsSome.ConfigureAwait(false)) Some(await opt.Value.ConfigureAwait(false)); else None();
            return Unit.Default;
        }

        public async Task<Unit> MatchAsync(TaskOption<A> opt, Func<A, Task> SomeAsync, Action None)
        {
            if (SomeAsync == null) throw new ArgumentNullException(nameof(SomeAsync));
            if (None == null) throw new ArgumentNullException(nameof(None));
            if (await opt.IsSome.ConfigureAwait(false)) await SomeAsync(await opt.Value.ConfigureAwait(false)); else None();
            return Unit.Default;
        }

        public async Task<Unit> MatchAsync(TaskOption<A> opt, Action<A> Some, Func<Task> NoneAsync)
        {
            if (Some == null) throw new ArgumentNullException(nameof(Some));
            if (NoneAsync == null) throw new ArgumentNullException(nameof(NoneAsync));
            if (await opt.IsSome.ConfigureAwait(false)) Some(await opt.Value.ConfigureAwait(false)); else await NoneAsync().ConfigureAwait(false);
            return Unit.Default;
        }

        public async Task<Unit> MatchAsync(TaskOption<A> opt, Func<A, Task> SomeAsync, Func<Task> NoneAsync)
        {
            if (SomeAsync == null) throw new ArgumentNullException(nameof(SomeAsync));
            if (NoneAsync == null) throw new ArgumentNullException(nameof(NoneAsync));
            if (await opt.IsSome.ConfigureAwait(false)) await SomeAsync(await opt.Value.ConfigureAwait(false)); else await NoneAsync().ConfigureAwait(false);
            return Unit.Default;
        }


        [Pure]
        public async Task<B> MatchUnsafe<B>(TaskOption<A> opt, Func<A, B> Some, Func<B> None)
        {
            if (Some == null) throw new ArgumentNullException(nameof(Some));
            if (None == null) throw new ArgumentNullException(nameof(None));
            return await opt.IsSome.ConfigureAwait(false)
                ? Some(await opt.Value.ConfigureAwait(false))
                : None();
        }

        [Pure]
        public async Task<B> MatchUnsafeAsync<B>(TaskOption<A> opt, Func<A, Task<B>> SomeAsync, Func<B> None)
        {
            if (SomeAsync == null) throw new ArgumentNullException(nameof(SomeAsync));
            if (None == null) throw new ArgumentNullException(nameof(None));
            return await opt.IsSome.ConfigureAwait(false)
                ? await SomeAsync(await opt.Value.ConfigureAwait(false))
                : None();
        }

        [Pure]
        public async Task<B> MatchUnsafeAsync<B>(TaskOption<A> opt, Func<A, B> Some, Func<Task<B>> NoneAsync)
        {
            if (Some == null) throw new ArgumentNullException(nameof(Some));
            if (NoneAsync == null) throw new ArgumentNullException(nameof(NoneAsync));
            return await opt.IsSome.ConfigureAwait(false)
                ? Some(await opt.Value.ConfigureAwait(false))
                : await NoneAsync().ConfigureAwait(false);
        }

        [Pure]
        public async Task<B> MatchUnsafeAsync<B>(TaskOption<A> opt, Func<A, Task<B>> SomeAsync, Func<Task<B>> NoneAsync)
        {
            if (SomeAsync == null) throw new ArgumentNullException(nameof(SomeAsync));
            if (NoneAsync == null) throw new ArgumentNullException(nameof(NoneAsync));
            return await opt.IsSome.ConfigureAwait(false)
                ? await SomeAsync(await opt.Value.ConfigureAwait(false)).ConfigureAwait(false)
                : await NoneAsync().ConfigureAwait(false);
        }

        [Pure]
        public TaskOption<A> Some(A value) =>
            isnull(value)
                ? throw new ArgumentNullException(nameof(value))
                : TaskOption<A>.Some(value);

        [Pure]
        public TaskOption<A> SomeAsync(Task<A> taskA) =>
            isnull(taskA)
                ? throw new ArgumentNullException(nameof(taskA))
                : TaskOption<A>.SomeAsync(taskA);

        [Pure]
        public TaskOption<A> Optional(A value) => TaskOption<A>.Optional(value);
        [Pure]
        public TaskOption<A> OptionalAsync(Task<A> taskA) => TaskOption<A>.OptionalAsync(taskA);

        [Pure]
        public TaskOption<A> BindReturn(Unit _, TaskOption<A> mb) => mb;
        [Pure]
        public TaskOption<A> ReturnAsync(Task<A> x) => ReturnAsync(_ => x);

        [Pure]
        public TaskOption<A> RunAsync(Func<Unit, Task<TaskOption<A>>> ma)
        {
            async Task<(bool IsSome, A Value)> Do(Func<Unit, Task<TaskOption<A>>> mma)
            {
                var a = await mma(unit).ConfigureAwait(false);
                return await a.Data.ConfigureAwait(false);
            }
            return new(Do(ma));
        }

        [Pure]
        public TaskOption<A> Empty() => None;

        [Pure]
        public TaskOption<A> Append(TaskOption<A> x, TaskOption<A> y) => Plus(x, y);

        [Pure]
        public Func<Unit, Task<S>> Fold<S>(TaskOption<A> ma, S state, Func<S, A, S> f) => async _ =>
        {
            if (state.IsNull()) throw new ArgumentNullException(nameof(state));
            f = f ?? throw new ArgumentNullException(nameof(f));
            return CheckNullReturn(await ma.IsSome.ConfigureAwait(false)
                ? f(state, await ma.Value.ConfigureAwait(false))
                : state);
        };

        [Pure]
        public Func<Unit, Task<S>> FoldAsync<S>(TaskOption<A> ma, S state, Func<S, A, Task<S>> f) => async _ =>
        {
            if (state.IsNull()) throw new ArgumentNullException(nameof(state));
            f = f ?? throw new ArgumentNullException(nameof(f));
            return CheckNullReturn(await ma.IsSome.ConfigureAwait(false)
                ? await f(state, await ma.Value.ConfigureAwait(false))
                : state);
        };

        [Pure]
        public Func<Unit, Task<S>> FoldBack<S>(TaskOption<A> ma, S state, Func<S, A, S> f) =>
            Fold(ma, state, f);

        [Pure]
        public Func<Unit, Task<S>> FoldBackAsync<S>(TaskOption<A> ma, S state, Func<S, A, Task<S>> f) =>
            FoldAsync(ma, state, f);

        [Pure]
        public Func<Unit, Task<int>> Count(TaskOption<A> ma) => async _ =>
            await ma.IsSome.ConfigureAwait(false) ? 1 : 0;

        [Pure]
        public async Task<S> BiFold<S>(TaskOption<A> ma, S state, Func<S, A, S> fa, Func<S, Unit, S> fb)
        {
            if (state.IsNull()) throw new ArgumentNullException(nameof(state));
            if (fa == null) throw new ArgumentNullException(nameof(fa));
            if (fb == null) throw new ArgumentNullException(nameof(fb));
            return CheckNullReturn(await ma.IsSome.ConfigureAwait(false)
                ? fa(state, await ma.Value.ConfigureAwait(false))
                : fb(state, unit));
        }

        [Pure]
        public async Task<S> BiFoldAsync<S>(TaskOption<A> ma, S state, Func<S, A, S> fa, Func<S, Unit, Task<S>> fb)
        {
            if (state.IsNull()) throw new ArgumentNullException(nameof(state));
            if (fa == null) throw new ArgumentNullException(nameof(fa));
            if (fb == null) throw new ArgumentNullException(nameof(fb));
            return CheckNullReturn(await ma.IsSome.ConfigureAwait(false)
                ? fa(state, await ma.Value.ConfigureAwait(false))
                : await fb(state, unit).ConfigureAwait(false));
        }

        [Pure]
        public async Task<S> BiFoldAsync<S>(TaskOption<A> ma, S state, Func<S, A, Task<S>> fa, Func<S, Unit, S> fb)
        {
            if (state.IsNull()) throw new ArgumentNullException(nameof(state));
            if (fa == null) throw new ArgumentNullException(nameof(fa));
            if (fb == null) throw new ArgumentNullException(nameof(fb));
            return CheckNullReturn(await ma.IsSome.ConfigureAwait(false)
                ? await fa(state, await ma.Value.ConfigureAwait(false))
                : fb(state, unit));
        }

        [Pure]
        public async Task<S> BiFoldAsync<S>(TaskOption<A> ma, S state, Func<S, A, Task<S>> fa, Func<S, Unit, Task<S>> fb)
        {
            if (state.IsNull()) throw new ArgumentNullException(nameof(state));
            if (fa == null) throw new ArgumentNullException(nameof(fa));
            if (fb == null) throw new ArgumentNullException(nameof(fb));
            return CheckNullReturn(await ma.IsSome.ConfigureAwait(false)
                ? await fa(state, await ma.Value.ConfigureAwait(false)).ConfigureAwait(false)
                : await fb(state, unit).ConfigureAwait(false));
        }

        [Pure]
        public Task<S> BiFoldBack<S>(TaskOption<A> ma, S state, Func<S, A, S> fa, Func<S, Unit, S> fb) =>
            BiFold(ma, state, fa, fb);

        [Pure]
        public Task<S> BiFoldBackAsync<S>(TaskOption<A> ma, S state, Func<S, A, S> fa, Func<S, Unit, Task<S>> fb) =>
            BiFoldAsync(ma, state, fa, fb);

        [Pure]
        public Task<S> BiFoldBackAsync<S>(TaskOption<A> ma, S state, Func<S, A, Task<S>> fa, Func<S, Unit, S> fb) =>
            BiFoldAsync(ma, state, fa, fb);

        [Pure]
        public Task<S> BiFoldBackAsync<S>(TaskOption<A> ma, S state, Func<S, A, Task<S>> fa, Func<S, Unit, Task<S>> fb) =>
            BiFoldAsync(ma, state, fa, fb);

        [Pure]
        public TaskOption<A> Apply(Func<A, A, A> f, TaskOption<A> fa, TaskOption<A> fb) =>
            default(MTaskOption<A>).RunAsync( async _ =>
            {
                var somes = await Task.WhenAll(fa.IsSome, fb.IsSome).ConfigureAwait(false);
                if (!somes[0] || !somes[1]) return TaskOption<A>.None;
                var values = await Task.WhenAll(fa.Value, fb.Value).ConfigureAwait(false);
                return f(values[0], values[1]);
            });

        [Pure]
        static T CheckNullReturn<T>(T value) => isnull(value) ? raise<T>(new ResultIsNullException()) : value;
    }
}