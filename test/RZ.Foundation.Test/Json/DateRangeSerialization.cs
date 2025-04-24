using System;
using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Extensions;
using RZ.Foundation.Types;
using Xunit;

namespace RZ.Foundation.Json;

public class DateRangeSerialization
{
    static readonly DateTime StartValue = 1.January(2025);
    static readonly DateTime EndValue = 15.January(2025);

    [Fact(DisplayName = "Serialize a limit duration")]
    public void Serialization() {
        var data = new DateRange(StartValue, EndValue);

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        json.Should().Be("""{"begin":"2025-01-01T00:00:00","end":"2025-01-15T00:00:00"}""", $"but {json}");
    }

    [Fact(DisplayName = "Serialize an open-ended duration")]
    public void SerializationOpenEnd() {
        var data = new DateRange(StartValue);

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        json.Should().Be("""{"begin":"2025-01-01T00:00:00"}""");
    }

    [Fact(DisplayName = "Serialize an open-began duration")]
    public void SerializationOpenBegin() {
        var data = new DateRange(end: EndValue);

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        json.Should().Be("""{"end":"2025-01-15T00:00:00"}""");
    }

    [Fact(DisplayName = "Serialize unlimit duration")]
    public void SerializationUnlimit() {
        var data = new DateRange();

        var json = JsonSerializer.Serialize(data, RzRecommendedJsonOptions);

        json.Should().Be("{}");
    }

    [Fact(DisplayName = "Deserialize a limit duration")]
    public void Deserialization() {
        var data = """{"begin":"2025-01-01T00:00:00","end":"2025-01-15T00:00:00"}""";

        var json = JsonSerializer.Deserialize<DateRange>(data, RzRecommendedJsonOptions);

        json.Should().Be(new DateRange(StartValue, EndValue));
    }

    [Fact(DisplayName = "Deserialize an open-ended duration")]
    public void DeserializationOpenEnd() {
        var data = """{"begin":"2025-01-01T00:00:00"}""";

        var json = JsonSerializer.Deserialize<DateRange>(data, RzRecommendedJsonOptions);

        json.Should().Be(new DateRange(StartValue));
    }

    [Fact(DisplayName = "Deserialize an open-began duration")]
    public void DeserializationOpenBegin() {
        var data = """{"end":"2025-01-15T00:00:00"}""";

        var json = JsonSerializer.Deserialize<DateRange>(data, RzRecommendedJsonOptions);

        json.Should().Be(new DateRange(end: EndValue));
    }

    [Fact(DisplayName = "Deserialize unlimit duration")]
    public void DeserializationUnlimit() {
        var data = "{}";

        var json = JsonSerializer.Deserialize<DateRange>(data, RzRecommendedJsonOptions);

        json.Should().Be(new DateRange());
    }
}