using System;
using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Extensions
{
    public sealed class UriExtensionTest
    {
        [Fact]
        public void Combine_with_empty() {
            var uri = new Uri("https://www.test.org/");
            var result = uri.Combine(string.Empty);
            result.Should().Be(new Uri("https://www.test.org/"));
        }
        [Fact]
        public void Combine_with_empty_no_slash() {
            var uri = new Uri("https://www.test.org");
            var result = uri.Combine(string.Empty);
            result.Should().Be(new Uri("https://www.test.org/"));
        }
        [Fact]
        public void Combine_with_path() {
            var uri = new Uri("https://www.test.org/");
            var result = uri.Combine("path1", "path2");
            result.Should().Be(new Uri("https://www.test.org/path1/path2"));
        }
        [Fact]
        public void Combine_with_query_string() {
            var uri = new Uri("https://www.test.org/path?param1=abc&param2=def");
            var result = uri.Combine("path1", "path2");
            result.Should().Be(new Uri("https://www.test.org/path/path1/path2?param1=abc&param2=def"));
        }
        [Fact]
        public void Combine_with_fragment() {
            var uri = new Uri("https://www.test.org/path#bookmark");
            var result = uri.Combine("path1", "path2");
            result.Should().Be(new Uri("https://www.test.org/path/path1/path2#bookmark"));
        }
        [Fact]
        public void Combine_with_query_string_and_fragment() {
            var uri = new Uri("https://www.test.org/path?param1=abc&param2=def#bookmark");
            var result = uri.Combine("path1", "path2");
            result.Should().Be(new Uri("https://www.test.org/path/path1/path2?param1=abc&param2=def#bookmark"));
        }
    }
}