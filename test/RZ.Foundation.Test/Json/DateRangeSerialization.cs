using System;
using System.Text.Json;
using RZ.Foundation.Types;

namespace RZ.Foundation.Json;

public class DateRangeSerialization
{
    static readonly DateTime StartValue = new(2025, 1, 1);
    static readonly DateTime EndValue = new(2025, 1, 15);

    [Test]
    [DisplayName("Serialize a limit duration")]
    public async Task Serialization() {
        var data = new DateRange(StartValue, EndValue);

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo("""{"begin":"2025-01-01T00:00:00","end":"2025-01-15T00:00:00"}""");
    }

    [Test]
    [DisplayName("Serialize an open-ended duration")]
    public async Task SerializationOpenEnd() {
        var data = new DateRange(StartValue);

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo("""{"begin":"2025-01-01T00:00:00"}""");
    }

    [Test]
    [DisplayName("Serialize an open-began duration")]
    public async Task SerializationOpenBegin() {
        var data = new DateRange(end: EndValue);

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo("""{"end":"2025-01-15T00:00:00"}""");
    }

    [Test]
    [DisplayName("Serialize unlimit duration")]
    public async Task SerializationUnlimit() {
        var data = new DateRange();

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo("{}");
    }

    [Test]
    [DisplayName("Deserialize a limit duration")]
    public async Task Deserialization() {
        var data = """{"begin":"2025-01-01T00:00:00","end":"2025-01-15T00:00:00"}""";

        var json = JsonSerializer.Deserialize<DateRange>(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo(new DateRange(StartValue, EndValue));
    }

    [Test]
    [DisplayName("Deserialize an open-ended duration")]
    public async Task DeserializationOpenEnd() {
        var data = """{"begin":"2025-01-01T00:00:00"}""";

        var json = JsonSerializer.Deserialize<DateRange>(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo(new DateRange(StartValue));
    }

    [Test]
    [DisplayName("Deserialize an open-began duration")]
    public async Task DeserializationOpenBegin() {
        var data = """{"end":"2025-01-15T00:00:00"}""";

        var json = JsonSerializer.Deserialize<DateRange>(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo(new DateRange(end: EndValue));
    }

    [Test]
    [DisplayName("Deserialize unlimit duration")]
    public async Task DeserializationUnlimit() {
        var data = "{}";

        var json = JsonSerializer.Deserialize<DateRange>(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo(new DateRange());
    }
}
