using System;
using System.Threading.Tasks;
using FluentAssertions;
using LanguageExt;
using RZ.Foundation.Extensions;
using RZ.Foundation.Functional.TaskOption;
using Xunit;
using static LanguageExt.Prelude;

namespace RZ.Foundation
{
    public class TestTaskOption
    {
        #region Async Pattern

        [Fact]
        public async Task CallSingleTaskOption() {
            var result = await GetInt();
            result.Should<Option<int>>().Be(Some(1));
        }

        [Fact]
        public async Task CallNestedTaskOption() {
            var result = await GetIntAsync();
            result.Should<Option<int>>().Be(Some(2));
        }

        [Fact]
        public async Task ReturnNoneFromTaskOption() {
            var result = await ReturnNone();
            result.Should<Option<int>>().Be(None);
        }

        [Fact]
        public async Task ExceptionIsEscalatedNormally() {
            Func<Task> act = async () => await ThrowException();
            await act.Should().ThrowExactlyAsync<NotImplementedException>();
        }

        static async TaskOption<int> GetIntAsync() {
            var value = await GetInt();
            return value.Map(i => i + 1).Get();
        }

        static async TaskOption<int> GetInt() {
            await Task.Yield();
            return 1;
        }

        static async TaskOption<int> ReturnNone() {
            await Task.Yield();
            return await TaskOptionNone<int>.Value;
        }

        static async TaskOption<int> ThrowException() {
            await Task.Yield();
            throw new NotImplementedException();
        }

        #endregion
    }
}