using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Extensions;
using RZ.Foundation.Types;
using Xunit;

namespace RZ.Foundation.Json;

public class TimeRangeSerialization
{
    [Fact(DisplayName = "Serialize a limit duration")]
    public void Serialization() {
        var data = new TimeRange(1.Hours(), 14.Hours());

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        json.Should().Be("""{"begin":"01:00:00","end":"14:00:00"}""");
    }

    [Fact(DisplayName = "Serialize an open-ended duration")]
    public void SerializationOpenEnd() {
        var data = new TimeRange(1.Hours());

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        json.Should().Be("""{"begin":"01:00:00"}""");
    }

    [Fact(DisplayName = "Serialize an open-began duration")]
    public void SerializationOpenBegin() {
        var data = new TimeRange(end: 14.Hours());

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        json.Should().Be("""{"end":"14:00:00"}""");
    }

    [Fact(DisplayName = "Serialize unlimit duration")]
    public void SerializationUnlimit() {
        var data = new TimeRange();

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        json.Should().Be("{}");
    }

    [Fact(DisplayName = "Deserialize a limit duration")]
    public void Deserialization() {
        var data = """{"begin":"01:00:00","end":"14:00:00"}""";

        var json = JsonSerializer.Deserialize<TimeRange>(data, RzRecommendedJsonOptions);

        json.Should().Be(new TimeRange(1.Hours(), 14.Hours()));
    }

    [Fact(DisplayName = "Deserialize an open-ended duration")]
    public void DeserializationOpenEnd() {
        var data = """{"begin":"01:00:00"}""";

        var json = JsonSerializer.Deserialize<TimeRange>(data, RzRecommendedJsonOptions);

        json.Should().Be(new TimeRange(1.Hours()));
    }

    [Fact(DisplayName = "Deserialize an open-began duration")]
    public void DeserializationOpenBegin() {
        var data = """{"end":"14:00:00"}""";

        var json = JsonSerializer.Deserialize<TimeRange>(data, RzRecommendedJsonOptions);

        json.Should().Be(new TimeRange(end: 14.Hours()));
    }

    [Fact(DisplayName = "Deserialize unlimit duration")]
    public void DeserializationUnlimit() {
        var data = "{}";

        var json = JsonSerializer.Deserialize<TimeRange>(data, RzRecommendedJsonOptions);

        json.Should().Be(new TimeRange());
    }
}