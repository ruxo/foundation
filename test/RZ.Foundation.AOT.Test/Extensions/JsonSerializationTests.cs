using System.Diagnostics;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using RZ.Foundation.Json;

namespace RZ.Foundation.Test.Extensions;

[UsedImplicitly]
public sealed class JsonSerializationTests
{
    readonly record struct MyData(string Name, int Age);

    [Test]
    public async ValueTask JsonNodeDeserialization() {
        var source = JsonNode.Parse("""{"name": "John", "age": 30}""");

        Debug.Assert(source is not null);
        var result = Success(source.TryDeserialize<MyData>(), out var data);

        await Assert.That(result).IsTrue();
        await Assert.That(data.Name).IsEqualTo("John");
        await Assert.That(data.Age).IsEqualTo(30);
    }
}