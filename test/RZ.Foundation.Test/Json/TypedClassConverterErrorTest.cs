using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace RZ.Foundation.Json;

#if NET9_0_OR_GREATER

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TypedClassConverterErrorTest
{
    enum AnimalType
    {
        [JsonStringEnumMemberName("dog")] Dog,
        [JsonStringEnumMemberName("cat")] Cat
    }

    abstract record Animal(AnimalType Type);

    [RzJsonDerivedType(AnimalType.Dog)]
    sealed record Dog(string Name) : Animal(AnimalType.Dog);

    static readonly JsonSerializerOptions Options = new JsonSerializerOptions {
        Converters = { new TypedClassConverter([typeof(Animal).Assembly]) }
    }.UseRzRecommendedSettings();

    [Test]
    [DisplayName("Missing discriminator throws JsonException (not NullReferenceException)")]
    public async Task MissingDiscriminatorThrowsJsonException() {
        // Discriminator property "type" omitted entirely.
        var json = """{"name":"Rex"}""";
        Action action = () => JsonSerializer.Deserialize<Animal>(json, Options);

        await Assert.That(action).Throws<JsonException>();
    }

    [Test]
    [DisplayName("Unknown discriminator value throws JsonException (not InvalidOperationException)")]
    public async Task UnknownDiscriminatorThrowsJsonException() {
        // Discriminator present but "cat" matches no registered subtype.
        var json = """{"type":"cat","name":"Rex"}""";
        Action action = () => JsonSerializer.Deserialize<Animal>(json, Options);

        await Assert.That(action).Throws<JsonException>();
    }

    [Test]
    [DisplayName("Valid discriminator still deserializes correctly")]
    public async Task ValidDiscriminatorStillWorks() {
        var json = """{"type":"dog","name":"Rex"}""";
        var animal = JsonSerializer.Deserialize<Animal>(json, Options);

        await Assert.That(animal).IsTypeOf<Dog>();
        await Assert.That(((Dog)animal!).Name).IsEqualTo("Rex");
    }
}

#endif
