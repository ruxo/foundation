using System;
using FluentAssertions;
using Xunit;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation
{
    public class ResultHelperTests
    {
        [Fact]
        public void Get_ResultRefValue() => success("hello").Get().Should().Be("hello");

        [Fact]
        public void Get_ResultValue() => success(1234).Get().Should().Be(1234);

        [Fact] public void Get_ResultGetValue_Faulted_Throw() => Get_ResultFaulted_Throw<string>();
        [Fact] public void Get_ResultValue_Faulted_Throw() => Get_ResultFaulted_Throw<int>();

        static void Get_ResultFaulted_Throw<T>() {
            Action act = () => faulted<T>(new Exception()).Get();

            act.Should().Throw<Exception>().WithMessage("Unhandle*");
        }

        [Fact] public void GetOrDefault_WithSuccessRef() => GetOrDefault_WithSuccess("hello");

        [Fact] public void GetOrDefault_WithSuccessValue() => GetOrDefault_WithSuccess(1234);
        static void GetOrDefault_WithSuccess<T>(T value) =>
            success(value).GetOrDefault().Should().Be(value);

        [Fact]
        public void GetOrDefault_WithFaultedRef() => GetOrDefault_WithFaulted<string>();

        [Fact]
        public void GetOrDefault_WithFaultedValue() => GetOrDefault_WithFaulted<int>();
        static void GetOrDefault_WithFaulted<T>() =>
            faulted<T>(new Exception()).GetOrDefault().Should().Be(default(T));
    }
}