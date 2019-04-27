using System;
using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;

namespace RZ.Foundation
{
    public class OptionHelperTests
    {
        [Fact] public void Get_Option_RefValue() => Some("hello").Get().Should().Be("hello");

        [Fact] public void Get_Option_Value() => Some(1234).Get().Should().Be(1234);

        [Fact] public void Get_Option_RefValue_None_Throw() {
            Action act = () => Option<string>.None.Get();
            act.Should().Throw<Exception>().WithMessage("Unhandled*");
        }

        [Fact] public void Get_Option_Value_None_Throw() {
            Action act = () => Option<int>.None.Get();
            act.Should().Throw<Exception>().WithMessage("Unhandled*");
        }

        [Fact]
        public void GetOrDefault_OptionRefValue_None_ReturnNull() =>
            Option<string>.None.GetOrDefault().Should().BeNull();

        [Fact]
        public void GetOrDefault_OptionValue_None_ReturnDefault() =>
            Option<int>.None.GetOrDefault().Should().Be(0);
    }
}