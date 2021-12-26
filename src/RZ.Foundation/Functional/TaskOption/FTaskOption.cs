using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.TypeClasses;
using static LanguageExt.Prelude;

namespace RZ.Foundation.Functional.TaskOption
{
    public struct FTaskOption<A, B> :
        FunctorAsync<TaskOption<A>, TaskOption<B>, A, B>,
        BiFunctorAsync<TaskOption<A>, TaskOption<B>, A, Unit, B>
    {
        public static readonly FTaskOption<A, B> Inst = default;

        [Pure]
        public TaskOption<B> BiMapAsync(TaskOption<A> ma, Func<A, B> fa, Func<Unit, B> fb)
        {
            async Task<(bool IsSome, B Value)> Do(TaskOption<A> mma, Func<A, B> ffa, Func<Unit, B> ffb) =>
                await mma.Match(
                    Some: x  => (true, ffa(x)),
                    None: () => (true, ffb(unit))).ConfigureAwait(false);

            return new(Do(ma, fa, fb));
        }

        [Pure]
        public TaskOption<B> BiMapAsync(TaskOption<A> ma, Func<A, Task<B>> fa, Func<Unit, B> fb)
        {
            async Task<(bool IsSome, B Value)> Do(TaskOption<A> mma, Func<A, Task<B>> ffa, Func<Unit, B> ffb) =>
                await mma.MatchAsync(
                    Some: async x => (true, await ffa(x)),
                    None: ()      => (true, ffb(unit))).ConfigureAwait(false);

            return new(Do(ma, fa, fb));
        }

        [Pure]
        public TaskOption<B> BiMapAsync(TaskOption<A> ma, Func<A, B> fa, Func<Unit, Task<B>> fb)
        {
            async Task<(bool IsSome, B Value)> Do(TaskOption<A> mma, Func<A, B> ffa, Func<Unit, Task<B>> ffb) =>
                await mma.MatchAsync(
                    Some: x        => (true, ffa(x)),
                    None: async () => (true, await ffb(unit))).ConfigureAwait(false);

            return new(Do(ma, fa, fb));
        }

        [Pure]
        public TaskOption<B> BiMapAsync(TaskOption<A> ma, Func<A, Task<B>> fa, Func<Unit, Task<B>> fb)
        {
            async Task<(bool IsSome, B Value)> Do(TaskOption<A> mma, Func<A, Task<B>> ffa, Func<Unit, Task<B>> ffb) =>
                await mma.MatchAsync(
                    Some: async x  => (true, await ffa(x)),
                    None: async () => (true, await ffb(unit))).ConfigureAwait(false);

            return new(Do(ma, fa, fb));
        }

        [Pure]
        public TaskOption<B> Map(TaskOption<A> ma, Func<A, B> f) =>
            default(MTaskOption<A>).Bind<MTaskOption<B>, TaskOption<B>, B>(ma,
                a => default(MTaskOption<B>).ReturnAsync(f(a).AsTask()));

        [Pure]
        public TaskOption<B> MapAsync(TaskOption<A> ma, Func<A, Task<B>> f) =>
            default(MTaskOption<A>).Bind<MTaskOption<B>, TaskOption<B>, B>(ma,
                a => default(MTaskOption<B>).ReturnAsync(f(a)));
    }

}