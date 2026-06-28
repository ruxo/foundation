namespace RZ.Foundation.Types;

/// <summary>
/// Regression tests for issue #12: <see cref="ErrorInfo.GetHashCode"/> used
/// <c>Enumerable.Sum(e =&gt; e.GetHashCode())</c> over <c>Locations</c> and
/// <c>SubErrors</c>, which accumulates in a checked context and threw
/// <see cref="OverflowException"/> once the running sum exceeded int range.
/// </summary>
public sealed class ErrorInfoHashCodeTest
{
    static ErrorInfo BuildHeavilyTraced() {
        var err = ErrorInfo.New("E_SAMPLE", "something failed");
        // ErrorInfo.New already seeds one location, so Locations starts non-empty.
        // Each Trace() appends an ErrorInfoLocation; with many entries the old
        // checked Sum over (effectively random) hash codes would overflow.
        for (var i = 0; i < 100; i++)
            err = err.Trace($"step {i}");
        return err;
    }

    [Test]
    [DisplayName("GetHashCode does not overflow with many trace locations")]
    public async Task GetHashCodeDoesNotOverflowWithManyLocations() {
        var err = BuildHeavilyTraced();

        // With the buggy checked Sum this throws OverflowException.
        await Assert.That(() => err.GetHashCode()).ThrowsNothing();

        var set = new HashSet<ErrorInfo>();
        await Assert.That(() => set.Add(err)).ThrowsNothing();
    }

    [Test]
    [DisplayName("GetHashCode does not overflow with many sub-errors")]
    public async Task GetHashCodeDoesNotOverflowWithManySubErrors() {
        var subErrors = Enumerable.Range(0, 100)
                                  .Select(i => ErrorInfo.New($"E_SUB_{i}", $"sub error {i}"))
                                  .ToArray();
        var err = ErrorInfo.New("E_PARENT", "parent failed", subErrors: subErrors);

        await Assert.That(() => err.GetHashCode()).ThrowsNothing();
    }

    [Test]
    [DisplayName("Equal ErrorInfo values produce equal hash codes")]
    public async Task EqualErrorInfoProduceEqualHashCodes() {
        var a = BuildHeavilyTraced();
        var b = BuildHeavilyTraced();

        // Sanity: the two values are structurally equal.
        await Assert.That(a).IsEqualTo(b);
        // Equal values must yield equal hash codes (XOR fold is order-independent
        // and overflow-free, matching the original commutative semantics).
        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }
}
