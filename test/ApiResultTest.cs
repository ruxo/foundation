using System;
using FluentAssertions;
using Xunit;
using static RZ.Foundation.Prelude;

namespace RZ.Foundation
{
    public class ApiResultTest
    {
        [Fact]
        public void ApiResultEquality_SuccessEqualsSuccess() => Success(12).Equals(Success(12)).Should().BeTrue();

        [Fact]
        public void ApiResultEquality_FailEqualsFail() => Failed<int>(new Exception()).Equals(Failed<int>(new ArgumentException())).Should().BeTrue();
    }
}