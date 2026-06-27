using System.Text.Json;
using LanguageExt;
using static LanguageExt.Prelude;
using Map = LanguageExt.Map;

namespace RZ.Foundation.Json;

public sealed class MapJsonConverterTest
{
    static readonly JsonSerializerOptions TestOptions = new(){ Converters ={ MapJsonConverter.Default }  };

    [Test]
    public async Task SerializeTest() {
        var data = new Test{ Name = "John", Phones = Seq(("th", 123),("en", 456)).ToMap()};

        var result = JsonSerializer.Serialize(data, TestOptions);

        await Assert.That(result).IsEqualTo("{\"Name\":\"John\",\"Phones\":{\"en\":456,\"th\":123}}");
    }

    [Test]
    public async Task SerializeEmptyTest() {
        var data = new Test{ Name = "John", Phones = Map.empty<string,int>() };

        var result = JsonSerializer.Serialize(data, TestOptions);

        await Assert.That(result).IsEqualTo("{\"Name\":\"John\",\"Phones\":{}}");
    }

    [Test]
    public async Task DeserializeTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":{\"en\":456,\"th\":123}}", TestOptions);

        var expected = new Test{ Name = "John", Phones = Seq(("th", 123),("en", 456)).ToMap()};
        await Assert.That(data).IsEqualTo(expected);
    }

    [Test]
    public async Task DeserializeEmptyTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Phones\":{}}", TestOptions);

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
        public Map<string,int> Phones { get; init; }
    }
}
