using System;
using System.Text.Json;
using JetBrains.Annotations;

namespace RZ.Foundation.Json;

#if NET9_0_OR_GREATER

/// <summary>
/// Regression test for issue #4: the derived-type JSON converter threw when the
/// discriminator property held an integral number larger than <see cref="int.MaxValue"/>.
/// <see cref="JsonConverterHelper.GetObjectValue"/> must read such values as <c>long</c>
/// instead of throwing while attempting <c>GetValue&lt;int&gt;()</c>.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LargeDiscriminatorTest
{
    abstract record EventBase(long Type);

    // Discriminator value itself is a large integer (> int.MaxValue == 2147483647).
    // attr.Value is a boxed long, and GetObjectValue must also yield a boxed long for
    // this magnitude so the discriminator comparison matches (and does not throw).
    [RzJsonDerivedType(3000000000L)]
    sealed record BigEvent(long Type, string Name) : EventBase(Type);

    static readonly JsonSerializerOptions Options = new() {
        Converters = { new TypedClassConverter([typeof(EventBase).Assembly]) }
    };

    [Test]
    [DisplayName("Discriminator larger than int.MaxValue resolves instead of throwing")]
    public async Task DeserializeLargeDiscriminator() {
        var json = """{"Type":3000000000,"Name":"big"}""";

        var result = JsonSerializer.Deserialize<EventBase>(json, Options);

        await Assert.That(result).IsTypeOf<BigEvent>();
        await Assert.That(((BigEvent)result!).Type).IsEqualTo(3000000000L);
        await Assert.That(((BigEvent)result!).Name).IsEqualTo("big");
    }
}

#endif
