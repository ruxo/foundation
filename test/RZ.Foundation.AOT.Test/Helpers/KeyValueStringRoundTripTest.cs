namespace RZ.Foundation.Helpers;

// Regression for issue #13: KeyValueString.Parse corrupts/rejects values containing
// backslash or double-quote. The value after '=' must round-trip verbatim as a literal
// string instead of being reinterpreted as JSON via raw string concatenation.
public sealed class KeyValueStringRoundTripTest
{
    [Test]
    public async ValueTask BackslashValue_RoundTripsVerbatim() {
        // Source string: name=a\b -> value must stay the literal three chars a, '\', b.
        var result = KeyValueString.Parse(@"name=a\b");

        await Assert.That(result["name"]).IsEqualTo(@"a\b");
    }

    [Test]
    public async ValueTask BackslashValue_IsNotInterpretedAsEscape() {
        // Previously "a\b" became 'a' + U+0008 (backspace), i.e. chars [97, 8].
        var result = KeyValueString.Parse(@"name=a\b");

        await Assert.That(result["name"].Length).IsEqualTo(3);
        await Assert.That(result["name"][1]).IsEqualTo('\\');
    }

    [Test]
    public async ValueTask DoubleQuoteValue_ParsesWithoutThrowing() {
        // Previously this threw ArgumentException ("Malformed key/JSON-value pairs").
        var result = KeyValueString.Parse("name=a\"b");

        await Assert.That(result["name"]).IsEqualTo("a\"b");
    }

    [Test]
    public async ValueTask PlainValue_StillParses() {
        var result = KeyValueString.Parse("name=value");

        await Assert.That(result["name"]).IsEqualTo("value");
    }

    [Test]
    public async ValueTask KeyWithoutSeparator_YieldsEmptyValue() {
        var result = KeyValueString.Parse("flag");

        await Assert.That(result["flag"]).IsEqualTo(string.Empty);
    }

    [Test]
    public async ValueTask MultiplePairs_AreSplitOnSemicolon() {
        var result = KeyValueString.Parse(@"path=c:\tmp;name=value");

        await Assert.That(result["path"]).IsEqualTo(@"c:\tmp");
        await Assert.That(result["name"]).IsEqualTo("value");
    }
}
