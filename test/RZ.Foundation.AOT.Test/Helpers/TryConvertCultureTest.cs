using System.Globalization;
using System.Threading;

namespace RZ.Foundation.Helpers;

// Regression for issue #6: TryConvert numeric/date parsers must use a fixed (invariant)
// culture rather than the ambient thread culture, so results are deterministic across
// host locales. Each test temporarily switches to a comma-decimal locale (de-DE) and
// restores the original culture in a finally block.
public sealed class TryConvertCultureTest
{
    static async ValueTask WithCulture(CultureInfo culture, Func<ValueTask> body) {
        var original = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = culture;
        try {
            await body();
        }
        finally {
            Thread.CurrentThread.CurrentCulture = original;
        }
    }

    [Test]
    public async ValueTask ToDouble_UsesInvariantCulture_UnderCommaDecimalLocale() =>
        await WithCulture(new CultureInfo("de-DE"), async () => {
            // "1.5" must parse as one-point-five regardless of the de-DE convention
            // (where '.' is a group separator).
            await Assert.That(TryConvert.ToDouble("1.5").IfNone(double.NaN)).IsEqualTo(1.5d);
        });

    [Test]
    public async ValueTask ToDouble_CommaIsGroupSeparator_UnderCommaDecimalLocale() =>
        await WithCulture(new CultureInfo("de-DE"), async () => {
            // Invariant culture treats ',' as the thousands separator, so "1,5" => 15.
            await Assert.That(TryConvert.ToDouble("1,5").IfNone(double.NaN)).IsEqualTo(15d);
        });

    [Test]
    public async ValueTask ToSingle_UsesInvariantCulture_UnderCommaDecimalLocale() =>
        await WithCulture(new CultureInfo("de-DE"), async () => {
            await Assert.That(TryConvert.ToSingle("1.5").IfNone(float.NaN)).IsEqualTo(1.5f);
        });

    [Test]
    public async ValueTask ToDecimal_UsesInvariantCulture_UnderCommaDecimalLocale() =>
        await WithCulture(new CultureInfo("de-DE"), async () => {
            await Assert.That(TryConvert.ToDecimal("1.5").IfNone(decimal.MinValue)).IsEqualTo(1.5m);
        });

    [Test]
    public async ValueTask ToDateTime_UsesInvariantCulture_UnderCommaDecimalLocale() =>
        await WithCulture(new CultureInfo("de-DE"), async () => {
            // "01/02/2024" is month/day/year under the invariant culture => 2 Jan 2024.
            await Assert.That(TryConvert.ToDateTime("01/02/2024").IfNone(default(DateTime)))
                        .IsEqualTo(new DateTime(2024, 1, 2));
        });

    [Test]
    public async ValueTask ToDateTimeOffset_UsesInvariantCulture_UnderCommaDecimalLocale() =>
        await WithCulture(new CultureInfo("de-DE"), async () => {
            await Assert.That(TryConvert.ToDateTimeOffset("01/02/2024").IfNone(default(DateTimeOffset)).DateTime)
                        .IsEqualTo(new DateTime(2024, 1, 2));
        });

    [Test]
    public async ValueTask ToInt32_IsCultureIndependent_UnderCommaDecimalLocale() =>
        await WithCulture(new CultureInfo("de-DE"), async () => {
            // A plain integer parses identically regardless of host locale.
            await Assert.That(TryConvert.ToInt32("1234").IfNone(-1)).IsEqualTo(1234);
        });
}
