using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static RZ.Foundation.Extensions.RetryExtension;

namespace RZ.Foundation.Extensions
{
    public sealed class RetryExtensionTest
    {
        [Fact]
        public void RetryIndefinitelySync_SuccessCase() {
            var result = RetryIndefinitely(() => 0);

            result.Should().Be(0);
        }

        [Fact]
        public void RetryIndefinitelySync_Fail5Times() {
            int retry = 5, count = 0;

            int runner() {
                if (--retry > 0) {
                    ++count;
                    throw new Exception("Mock error");
                }
                return count;
            }

            var result = RetryIndefinitely(runner);

            result.Should().Be(4);
        }

        [Fact]
        public async Task RetryIndefinitelyAsync_Fail5Times() {
            int retry = 5, count = 0;

            async Task<int> runner() {
                await Task.Yield();
                if (--retry > 0) {
                    ++count;
                    throw new Exception();
                }

                return count;
            }

            var result = await RetryIndefinitely(runner);

            result.Should().Be(4);
        }

        [Fact]
        public async Task RetryUntilAsync_Fail5Times() {
            int retry = 5;

            var result = await RetryUntil(() => Task.FromException<int>(new Exception("TEST FAILED")), _ => --retry > 0);

            result.GetFail().Message.Should().Be("TEST FAILED");
        }
    }
}