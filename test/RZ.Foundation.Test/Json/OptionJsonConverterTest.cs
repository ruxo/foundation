using System.Text.Json;
using FluentAssertions;
using LanguageExt;
using Xunit;

namespace RZ.Foundation.Json;

public sealed class OptionJsonConverterTest
{
    static readonly JsonSerializerOptions TestOptions = new(){ Converters ={ OptionJsonConverter.Default } };
    
    [Fact]
    public void SerializeTest() {
        var data = new Test{ Name = "John", Age = 123, CitizenId = "112233" };

        var result = JsonSerializer.Serialize(data, TestOptions);

        result.Should().Be("{\"Name\":\"John\",\"Age\":123,\"CitizenId\":\"112233\"}");
    }
    
    [Fact]
    public void SerializeNullTest() {
        var data = new Test{ Name = "John" };

        var result = JsonSerializer.Serialize(data, TestOptions);

        result.Should().Be("{\"Name\":\"John\",\"Age\":null,\"CitizenId\":null}");
    }

    [Fact]
    public void DeserializeTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Age\":123,\"CitizenId\":\"112233\"}", TestOptions);
        
        var expected = new Test{ Name = "John", Age = 123, CitizenId = "112233" };
        data.Should().Be(expected);
    }

    [Fact]
    public void DeserializeNullTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Age\":null,\"CitizenId\":null}", TestOptions);

        var expected = new Test{ Name = "John" };
        data.Should().Be(expected);
    }

    readonly record struct Test
    {
        public required string Name { get; init; }
        public Option<int> Age { get; init; }
        public Option<string> CitizenId { get; init; }
    }
}