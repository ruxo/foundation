using System.Text.Json;
using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;
using Map = LanguageExt.Map;

namespace RZ.Foundation.Json;

public sealed class MapJsonConverterTest
{
    static readonly JsonSerializerOptions TestOptions = new(){ Converters ={ MapJsonConverter.Default }  };
    
    [Fact]
    public void SerializeTest() {
        var data = new Test{ Name = "John", Phones = Seq(("th", 123),("en", 456)).ToMap()};

        var result = JsonSerializer.Serialize(data, TestOptions);

        result.Should().Be("{\"Name\":\"John\",\"Phones\":{\"en\":456,\"th\":123}}");
    }
    
    [Fact]
    public void SerializeEmptyTest() {
        var data = new Test{ Name = "John", Phones = Map.empty<string,int>() };

        var result = JsonSerializer.Serialize(data, TestOptions);

        result.Should().Be("{\"Name\":\"John\",\"Phones\":{}}");
    }

    [Fact]
    public void DeserializeTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":{\"en\":456,\"th\":123}}", TestOptions);
        
        var expected = new Test{ Name = "John", Phones = Seq(("th", 123),("en", 456)).ToMap()};
        data.Should().Be(expected);
    }

    [Fact]
    public void DeserializeEmptyTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":{}}", TestOptions);
        
        var expected = new Test{ Name = "John" };
        data.Should().Be(expected);
    }

    [Fact]
    public void DeserializeMissingTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\"}", TestOptions);
        
        var expected = new Test{ Name = "John" };
        data.Should().Be(expected);
    }

    readonly record struct Test
    {
        public required string Name { get; init; }
        public Map<string,int> Phones { get; init; }
    }
}