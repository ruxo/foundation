using System.Text.Json;
using FluentAssertions;
using JetBrains.Annotations;
using RZ.Foundation.Json;
using RZ.Foundation.Types;
using Xunit;

namespace RZ.Foundation.Functional;

public sealed class OutcomeTest
{
    #region Serializable!

    [PublicAPI]
    sealed record TestRecord(string Name, int Age);

    [Fact]
    public void NativeSerializeSuccessOutcome() {
        Outcome<TestRecord> outcome = new TestRecord("John", 42);

        // when
        var json = JsonSerializer.Serialize(outcome);

        // then
        json.Should().Be("""{"Error":null,"Data":{"Name":"John","Age":42},"State":"success"}""");
    }

    [Fact]
    public void NativeDeserializeSuccessOutcome() {
        const string json = """{"Error":null,"Data":{"Name":"John","Age":42},"State":"success"}""";
        var expected = SuccessOutcome(new TestRecord("John", 42));

        // when
        var outcome = JsonSerializer.Deserialize<Outcome<TestRecord>>(json);

        // then
        outcome.Should().Be(expected);
    }

    [Fact]
    public void SerializeSuccessOutcomeToJson() {
        Outcome<TestRecord> outcome = new TestRecord("John", 42);

        // when
        var json = JsonSerializer.Serialize(outcome, new JsonSerializerOptions().UseRzConverters());

        // then
        json.Should().Be("{\"Data\":{\"Name\":\"John\",\"Age\":42}}");
    }

    [Fact]
    public void SerializeFailureOutcomeToJson() {
        Outcome<TestRecord> outcome = new ErrorInfo(StandardErrorCodes.Unhandled);

        // when
        var json = JsonSerializer.Serialize(outcome, new JsonSerializerOptions().UseRzConverters());

        // then
        json.Should().Be("""{"Error":{"Code":"unhandled","Message":"unhandled"}}""");
    }

    [Fact]
    public void DeserializeSuccessOutcomeToJson() {
        var json = "{\"Data\":{\"Name\":\"John\",\"Age\":42}}";
        Outcome<TestRecord> expected = new TestRecord("John", 42);

        // when
        var outcome = JsonSerializer.Deserialize<Outcome<TestRecord>>(json, new JsonSerializerOptions().UseRzConverters());

        // then
        outcome.Should().Be(expected);
    }

    [Fact]
    public void DeserializeFailureOutcomeToJson() {
        var json = "{\"Error\":{\"Code\":\"unhandled\",\"Message\":\"unhandled\",\"TraceId\":null,\"DebugInfo\":null,\"Data\":null}}";
        Outcome<TestRecord> expected = new ErrorInfo(StandardErrorCodes.Unhandled);

        // when
        var outcome = JsonSerializer.Deserialize<Outcome<TestRecord>>(json, new JsonSerializerOptions().UseRzConverters());

        // then
        outcome.Should().Be(expected);
    }

    #endregion
}