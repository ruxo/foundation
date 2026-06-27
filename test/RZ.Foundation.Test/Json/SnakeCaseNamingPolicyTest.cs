using System.Text.Json;

namespace RZ.Foundation.Json;

public sealed class SnakeCaseNamingPolicyTest
{
    [Test]
    public async Task Serialization() {
        var option = new JsonSerializerOptions{ PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance };

        var result = JsonSerializer.Serialize(new{ FirstName = "John", Age = 123 }, option);

        await Assert.That(result).IsEqualTo(@"{""first_name"":""John"",""age"":123}");
    }

    [Test]
    public async Task Deserialization() {
        var option = new JsonSerializerOptions{ PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance };

        var result = JsonSerializer.Deserialize<Data>(@"{""first_name"":""John"",""age"":123}", option);

        await Assert.That(result).IsEqualTo(new Data("John", 123));
    }

    readonly record struct Data(string FirstName, int Age);
}
