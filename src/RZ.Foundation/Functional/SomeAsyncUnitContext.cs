using System;
using System.Threading.Tasks;
using LanguageExt.TypeClasses;

namespace RZ.Foundation.Functional
{
    public sealed class SomeAsyncUnitContext<OPT, OA, A> where OPT : struct, OptionalAsync<OA, A>
    {
        readonly OA option;
        readonly Action<A> someHandler;
        Action noneHandler = null!;

        /// <summary>
        /// INTERNAL Use only
        /// </summary>
        /// <param name="option">An Option class.</param>
        /// <param name="someHandler">Default "Some" handler.</param>
        public SomeAsyncUnitContext(OA option, Action<A> someHandler) {
            this.option = option;
            this.someHandler = someHandler;
        }

        /// <summary>The None branch of the matching operation</summary>
        /// <param name="f">None branch operation</param>
        public Task<Unit> None(Action f) {
            noneHandler = f;
            return default(OPT).Match(option, HandleSome, HandleNone);
        }

        Unit HandleSome(A value) {
            someHandler(value);
            return unit;
        }

        Unit HandleNone() {
            noneHandler();
            return unit;
        }
    }
}