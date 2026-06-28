using System.Text.Json;
using JetBrains.Annotations;
using RZ.Foundation.Json;
using RZ.Foundation.Types;

namespace RZ.Foundation.Functional;

public sealed class OutcomeTest
{
    #region Serializable!

    [PublicAPI]
    sealed record TestRecord(string Name, int Age);

    [Test]
    public async Task NativeSerializeSuccessOutcome() {
        Outcome<TestRecord> outcome = new TestRecord("John", 42);

        // when
        var json = JsonSerializer.Serialize(outcome);

        // then
        await Assert.That(json).IsEqualTo("""{"Error":null,"Data":{"Name":"John","Age":42},"State":"success"}""");
    }

    [Test]
    public async Task NativeDeserializeSuccessOutcome() {
        const string json = """{"Error":null,"Data":{"Name":"John","Age":42},"State":"success"}""";
        var expected = SuccessOutcome(new TestRecord("John", 42));

        // when
        var outcome = JsonSerializer.Deserialize<Outcome<TestRecord>>(json);

        // then
        await Assert.That(outcome).IsEqualTo(expected);
    }

    [Test]
    public async Task SerializeSuccessOutcomeToJson() {
        Outcome<TestRecord> outcome = new TestRecord("John", 42);

        // when
        var json = JsonSerializer.Serialize(outcome, new JsonSerializerOptions().UseRzConverters());

        // then
        await Assert.That(json).IsEqualTo("{\"Data\":{\"Name\":\"John\",\"Age\":42}}");
    }

    [Test]
    public async Task SerializeFailureOutcomeToJson() {
        Outcome<TestRecord> outcome = new ErrorInfo(StandardErrorCodes.UNHANDLED) { TraceId = null };

        // when
        var json = JsonSerializer.Serialize(outcome, new JsonSerializerOptions().UseRzConverters());

        // then
        await Assert.That(json).IsEqualTo("""{"Error":{"Code":"unhandled","Message":"unhandled","Locations":[]}}""");
    }

    [Test]
    public async Task DeserializeSuccessOutcomeToJson() {
        var json = "{\"Data\":{\"Name\":\"John\",\"Age\":42}}";
        Outcome<TestRecord> expected = new TestRecord("John", 42);

        // when
        var outcome = JsonSerializer.Deserialize<Outcome<TestRecord>>(json, new JsonSerializerOptions().UseRzConverters());

        // then
        await Assert.That(outcome).IsEqualTo(expected);
    }

    [Test]
    public async Task DeserializeFailureOutcomeToJson() {
        var json = "{\"Error\":{\"Code\":\"unhandled\",\"Message\":\"unhandled\",\"TraceId\":null,\"DebugInfo\":null,\"Data\":null}}";
        Outcome<TestRecord> expected = new ErrorInfo(StandardErrorCodes.UNHANDLED) { TraceId = null };

        // when
        var outcome = JsonSerializer.Deserialize<Outcome<TestRecord>>(json, new JsonSerializerOptions().UseRzConverters());

        // then
        await Assert.That(outcome).IsEqualTo(expected);
    }

    #endregion
}
