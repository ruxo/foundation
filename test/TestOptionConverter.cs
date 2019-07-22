using FluentAssertions;
using Newtonsoft.Json;
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
    }
}
