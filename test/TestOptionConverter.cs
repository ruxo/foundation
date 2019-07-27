using System;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace RZ.Foundation
{
    public class TestOptionConverter
    {
        public class Sample
        {
            [JsonConverter(typeof(OptionConverter<string>))]
            public Option<string> OptionField { get; set; }
        }

        [Fact]
        public void SerializeStringWithOptionConverter()
        {
            var data = new Sample { OptionField = "Test" };

            var serialized = JsonConvert.SerializeObject(data);

            serialized.Should().Be(@"{""OptionField"":""Test""}");
        }

        [Fact]
        public void SerializeNullStringWithOptionConverter()
        {
            var data = new Sample { OptionField = Option<string>.None() };

            var serialized = JsonConvert.SerializeObject(data);

            serialized.Should().Be(@"{""OptionField"":null}");
        }

        [Fact]
        public void DeserializeStringWithOptionConverter()
        {
            var data = JsonConvert.DeserializeObject<Sample>(@"{""OptionField"":""Test""}");

            data.OptionField.Should().NotBeNull();
            data.OptionField.IsSome.Should().BeTrue();
            data.OptionField.GetOrElse("XXX").Should().Be("Test");
        }

        [Fact]
        public void DeserializeNullStringWithOptionConverter()
        {
            var data = JsonConvert.DeserializeObject<Sample>(@"{""OptionField"":null}");

            data.OptionField.Should().NotBeNull();
            data.OptionField.IsNone.Should().BeTrue();
        }

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy), ItemRequired = Required.Always)]
        public sealed class ComplexType
        {
            public string Value;
        }

        [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy), ItemRequired = Required.Always)]
        public sealed class ComplexWrapper
        {
            [JsonProperty(Required = Required.Default)]
            [JsonConverter(typeof(OptionConverter<ComplexType>))]
            public Option<ComplexType> ComplexDefault;
        }

        [Fact]
        public void TestDeserialize() {
            const string Load = @"{ ""complex_default"": { ""value"": ""abcdef"" }}";

            var result = JsonConvert.DeserializeObject<ComplexWrapper>(Load);

            result.ComplexDefault.IsSome.Should().BeTrue();
        }

        [Fact]
        public void TestDeserializeObjectWithNull() {
            const string Load = @"{""complex_default"": null}";

            var result = JsonConvert.DeserializeObject<ComplexWrapper>(Load);

            result.ComplexDefault.IsNone.Should().BeTrue();
        }

        [Fact]
        public void TestDeserializeObjectWithMissing() {
            const string Load = @"{}";

            var result = JsonConvert.DeserializeObject<ComplexWrapper>(Load);

            result.ComplexDefault.IsNone.Should().BeTrue();
        }
    }
}
