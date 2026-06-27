using System.Text.Json;
using LanguageExt;

namespace RZ.Foundation.Json;

public sealed class OptionJsonConverterTest
{
    static readonly JsonSerializerOptions TestOptions = new(){ Converters ={ OptionJsonConverter.Default } };

    [Test]
    public async Task SerializeTest() {
        var data = new Test{ Name = "John", Age = 123, CitizenId = "112233" };

        var result = JsonSerializer.Serialize(data, TestOptions);

        await Assert.That(result).IsEqualTo("{\"Name\":\"John\",\"Age\":123,\"CitizenId\":\"112233\"}");
    }

    [Test]
    public async Task SerializeNullTest() {
        var data = new Test{ Name = "John" };

        var result = JsonSerializer.Serialize(data, TestOptions);

        await Assert.That(result).IsEqualTo("{\"Name\":\"John\",\"Age\":null,\"CitizenId\":null}");
    }

    [Test]
    public async Task DeserializeTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Age\":123,\"CitizenId\":\"112233\"}", TestOptions);

        var expected = new Test{ Name = "John", Age = 123, CitizenId = "112233" };
        await Assert.That(data).IsEqualTo(expected);
    }

    [Test]
    public async Task DeserializeNullTest() {
        var data = JsonSerializer.Deserialize<Test>("{\"Name\":\"John\",\"Age\":null,\"CitizenId\":null}", TestOptions);

        var expected = new Test{ Name = "John" };
        await Assert.That(data).IsEqualTo(expected);
    }

    readonly record struct Test
    {
        public required string Name { get; init; }
        public Option<int> Age { get; init; }
        public Option<string> CitizenId { get; init; }
    }
}
