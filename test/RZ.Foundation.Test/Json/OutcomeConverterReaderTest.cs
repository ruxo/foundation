using System.Text.Json;

namespace RZ.Foundation.Json;

/// <summary>
/// Regression tests for issue #16: <see cref="OutcomeConverter"/> cleanup loop stops on the wrong
/// <c>EndObject</c> when a property follows <c>data</c>/<c>error</c>. The cleanup must be depth-aware so
/// the reader ends exactly past the Outcome object's closing brace, leaving any sibling content intact.
/// </summary>
public sealed class OutcomeConverterReaderTest
{
    // Case-insensitive so the wrapper's PascalCase properties (Result/Tag) bind to the lowercase JSON keys;
    // the OutcomeConverter itself already reads data/error case-insensitively.
    static readonly JsonSerializerOptions TestOptions = new(){ PropertyNameCaseInsensitive = true, Converters ={ new OutcomeConverter() } };

    [Test]
    public async Task DeserializeSuccessWithTrailingNestedObjectProperty() {
        // "data" is followed by another property ("meta") whose value is a nested object.
        const string json = """
        {
            "data": 42,
            "meta": { "source": "cache" }
        }
        """;

        var outcome = JsonSerializer.Deserialize<Outcome<int>>(json, TestOptions);

        await Assert.That(outcome.IfSuccess(out var v)).IsTrue();
        await Assert.That(v).IsEqualTo(42);
    }

    [Test]
    public async Task SiblingPropertyAfterOutcomeStillDeserializes() {
        // The Outcome<int> property has a trailing nested-object property ("meta") AFTER "data",
        // and is itself followed by a sibling property ("tag"). If the reader stops on the wrong
        // EndObject (the inner "meta" '}'), the sibling can no longer be read correctly.
        const string json = """
        {
            "result": {
                "data": 42,
                "meta": { "source": "cache" }
            },
            "tag": "after"
        }
        """;

        var wrapper = JsonSerializer.Deserialize<Wrapper>(json, TestOptions);

        await Assert.That(wrapper.Result.IfSuccess(out var v)).IsTrue();
        await Assert.That(v).IsEqualTo(42);
        // Proves the reader ended at the Outcome object's matching '}', not inside it.
        await Assert.That(wrapper.Tag).IsEqualTo("after");
    }

    [Test]
    public async Task SiblingPropertyAfterFailureOutcomeStillDeserializes() {
        // Same scenario on the failure branch: "error" followed by a trailing nested-object property.
        const string json = """
        {
            "result": {
                "error": { "Code": "not-found", "Message": "missing" },
                "meta": { "source": "cache" }
            },
            "tag": "after"
        }
        """;

        var wrapper = JsonSerializer.Deserialize<Wrapper>(json, TestOptions);

        await Assert.That(wrapper.Result.IfFail(out var e)).IsTrue();
        await Assert.That(e!.Code).IsEqualTo("not-found");
        await Assert.That(wrapper.Tag).IsEqualTo("after");
    }

    readonly record struct Wrapper
    {
        public Outcome<int> Result { get; init; }
        public string Tag { get; init; }
    }
}
