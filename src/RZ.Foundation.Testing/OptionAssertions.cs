using FluentAssertions;
using FluentAssertions.Execution;
using JetBrains.Annotations;

namespace RZ.Foundation.Testing;

[PublicAPI]
public static class OptionExtensions
{
    public static OptionAssertions<T> Should<T>(this Option<T> v) => new(v);
}

[PublicAPI]
public readonly ref struct OptionAssertions<T>(Option<T> v)
{
    public void BeSome(T value, string because = "", params object[] becauseArgs) {
        Execute.Assertion
               .BecauseOf(because, becauseArgs)
               .ForCondition(v == Some(value))
               .FailWith("Expect {context:option} to be some value of {0} {reason}, but found {1}", value, v);
    }

    public AndConstraint<PassValue> BeSomething(string because = "", params object[] becauseArgs) {
        Execute.Assertion
               .BecauseOf(because, becauseArgs)
               .ForCondition(v.IsSome)
               .FailWith("Expect {context:option} to be something {reason}, but found {0}", v);
        return new(new(v));
    }

    public void BeNone(string because = "", params object[] becauseArgs) {
        Execute.Assertion
               .BecauseOf(because, becauseArgs)
               .ForCondition(v.IsNone)
               .FailWith("Expect {context:option} to be nothing {reason}, but found {0}", v);
    }

    [PublicAPI]
    public readonly struct PassValue
    {
        readonly Option<T> target;
        internal PassValue(Option<T> target) {
            this.target = target;
        }

        public T Value => target.Get();
        public T It => target.Get();
        public T Its => target.Get();
    }
}