using System.Text.Json;
using LanguageExt;
using static LanguageExt.Prelude;
using Set = LanguageExt.Set;

namespace RZ.Foundation.Json;

public sealed class SetJsonConverterTest
{
    static readonly JsonSerializerOptions TestOptions = new(){ Converters ={ SetJsonConverter.Default }  };

    [Test]
    public async Task SerializeTest() {
        var data = new Test{ Name = "John", Phones = Set("123", "456")};

        var result = JsonSerializer.Serialize(data, TestOptions);

        await Assert.That(result).IsEqualTo("{\"Name\":\"John\",\"Phones\":[\"123\",\"456\"]}");
    }

    [Test]
    public async Task SerializeEmptyTest() {
        var data = new Test{ Name = "John", Phones = Set.empty<string>() };

        var result = JsonSerializer.Serialize(data, TestOptions);

        await Assert.That(result).IsEqualTo("{\"Name\":\"John\",\"Phones\":[]}");
    }

    [Test]
    public async Task DeserializeTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":[\"123\",\"456\"]}", TestOptions);

        var expected = new Test{ Name = "John", Phones = Set("123", "456") };
        await Assert.That(data).IsEqualTo(expected);
    }

    [Test]
    public async Task DeserializeEmptyTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":[]}", TestOptions);

        var expected = new Test{ Name = "John" };
        await Assert.That(data).IsEqualTo(expected);
    }

    [Test]
    public async Task DeserializeMissingTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\"}", TestOptions);

        var expected = new Test{ Name = "John" };
        await Assert.That(data).IsEqualTo(expected);
    }

    readonly record struct Test
    {
        public required string Name { get; init; }
        public Set<string> Phones { get; init; }
    }
}
