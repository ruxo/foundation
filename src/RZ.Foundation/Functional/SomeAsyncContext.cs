using System;
using System.Threading.Tasks;
using LanguageExt.TypeClasses;

namespace RZ.Foundation.Functional
{
    public sealed class SomeAsyncContext<OPT, OA, A, B> where OPT : struct, OptionalAsync<OA, A>
    {
        readonly OA option;
        readonly Func<A, B> someHandler;

        /// <summary>
        /// INTERNAL Use Only
        /// </summary>
        /// <param name="option">An Option instance.</param>
        /// <param name="someHandler">Default "Some" handler.</param>
        internal SomeAsyncContext(OA option, Func<A, B> someHandler)
        {
            this.option = option;
            this.someHandler = someHandler;
        }

        /// <summary>The None branch of the matching operation</summary>
        /// <param name="noneHandler">None branch operation</param>
        public Task<B> None(Func<B> noneHandler) => default (OPT).Match(option, someHandler, noneHandler);

        /// <summary>The None branch of the matching operation</summary>
        /// <param name="noneValue">None branch value</param>
        public Task<B> None(B noneValue) => default (OPT).Match(option, someHandler, () => noneValue);
    }
}