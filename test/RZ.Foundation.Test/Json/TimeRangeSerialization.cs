using System.Text.Json;
using LanguageExt.UnitsOfMeasure;
using RZ.Foundation.Types;

namespace RZ.Foundation.Json;

public class TimeRangeSerialization
{
    [Test]
    [DisplayName("Serialize a limit duration")]
    public async Task Serialization() {
        var data = new TimeRange(1.Hours(), 14.Hours());

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo("""{"begin":"01:00:00","end":"14:00:00"}""");
    }

    [Test]
    [DisplayName("Serialize an open-ended duration")]
    public async Task SerializationOpenEnd() {
        var data = new TimeRange(1.Hours());

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo("""{"begin":"01:00:00"}""");
    }

    [Test]
    [DisplayName("Serialize an open-began duration")]
    public async Task SerializationOpenBegin() {
        var data = new TimeRange(end: 14.Hours());

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo("""{"end":"14:00:00"}""");
    }

    [Test]
    [DisplayName("Serialize unlimit duration")]
    public async Task SerializationUnlimit() {
        var data = new TimeRange();

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo("{}");
    }

    [Test]
    [DisplayName("Deserialize a limit duration")]
    public async Task Deserialization() {
        var data = """{"begin":"01:00:00","end":"14:00:00"}""";

        var json = JsonSerializer.Deserialize<TimeRange>(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo(new TimeRange(1.Hours(), 14.Hours()));
    }

    [Test]
    [DisplayName("Deserialize an open-ended duration")]
    public async Task DeserializationOpenEnd() {
        var data = """{"begin":"01:00:00"}""";

        var json = JsonSerializer.Deserialize<TimeRange>(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo(new TimeRange(1.Hours()));
    }

    [Test]
    [DisplayName("Deserialize an open-began duration")]
    public async Task DeserializationOpenBegin() {
        var data = """{"end":"14:00:00"}""";

        var json = JsonSerializer.Deserialize<TimeRange>(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo(new TimeRange(end: 14.Hours()));
    }

    [Test]
    [DisplayName("Deserialize unlimit duration")]
    public async Task DeserializationUnlimit() {
        var data = "{}";

        var json = JsonSerializer.Deserialize<TimeRange>(data, RzRecommendedJsonOptions);

        await Assert.That(json).IsEqualTo(new TimeRange());
    }
}
