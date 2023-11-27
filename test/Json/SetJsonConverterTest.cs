using System.Text.Json;
using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;
using Set = LanguageExt.Set;

namespace RZ.Foundation.Json;

public sealed class SetJsonConverterTest
{
    static readonly JsonSerializerOptions TestOptions = new(){ Converters ={ SetJsonConverter.Default }  };
    
    [Fact]
    public void SerializeTest() {
        var data = new Test{ Name = "John", Phones = Set("123", "456")};

        var result = JsonSerializer.Serialize(data, TestOptions);

        result.Should().Be("{\"Name\":\"John\",\"Phones\":[\"123\",\"456\"]}");
    }
    
    [Fact]
    public void SerializeEmptyTest() {
        var data = new Test{ Name = "John", Phones = Set.empty<string>() };

        var result = JsonSerializer.Serialize(data, TestOptions);

        result.Should().Be("{\"Name\":\"John\",\"Phones\":[]}");
    }

    [Fact]
    public void DeserializeTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":[\"123\",\"456\"]}", TestOptions);
        
        var expected = new Test{ Name = "John", Phones = Set("123", "456") };
        data.Should().Be(expected);
    }

    [Fact]
    public void DeserializeEmptyTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":[]}", TestOptions);
        
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
        public Set<string> Phones { get; init; }
    }
}