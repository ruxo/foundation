using System.Text.Json;
using FluentAssertions;
using LanguageExt;
using Xunit;
using static LanguageExt.Prelude;
using Seq = LanguageExt.Seq;

namespace RZ.Foundation.Json;

public sealed class SeqJsonConverterTest
{
    static readonly JsonSerializerOptions TestOptions = new(){ Converters ={ SeqJsonConverter.Default }  };
    
    [Fact]
    public void SerializeTest() {
        var data = new Test{ Name = "John", Phones = Seq("123", "456")};

        var result = JsonSerializer.Serialize(data, TestOptions);

        result.Should().Be("{\"Name\":\"John\",\"Phones\":[\"123\",\"456\"]}");
    }
    
    [Fact]
    public void SerializeEmptyTest() {
        var data = new Test{ Name = "John", Phones = Seq.empty<string>() };

        var result = JsonSerializer.Serialize(data, TestOptions);

        result.Should().Be("{\"Name\":\"John\",\"Phones\":[]}");
    }

    [Fact]
    public void DeserializeTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":[\"123\",\"456\"]}", TestOptions);
        
        var expected = new Test{ Name = "John", Phones = Seq("123", "456") };
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
        public Seq<string> Phones { get; init; }
    }
}