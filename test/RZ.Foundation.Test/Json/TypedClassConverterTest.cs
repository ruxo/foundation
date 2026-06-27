using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace RZ.Foundation.Json;

#if NET9_0_OR_GREATER

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TypedClassConverterTest
{
    enum PersonType
    {
        [JsonStringEnumMemberName("student")] Student,
        [JsonStringEnumMemberName("teacher")] Teacher,
        [JsonStringEnumMemberName("accountant")] Accountant
    }

    abstract record Person(PersonType Type);

    [RzJsonDerivedType(PersonType.Student)]
    sealed record Student(string Id) : Person(PersonType.Student);

    [RzJsonDerivedType(PersonType.Teacher)]
    record Teacher(PersonType Type, string Subject) : Person(Type)
    {
        [JsonConstructor]
        public Teacher(string subject) : this(PersonType.Teacher, subject) { }
    }

    static readonly JsonSerializerOptions Options = new JsonSerializerOptions {
        Converters = { new TypedClassConverter([typeof(Person).Assembly]) }
    }.UseRzRecommendedSettings();

    #region Basic cases

    [Test]
    public async Task DeserializeStudent() {
        var json = """{"type":"student","id":"42"}""";
        var student = JsonSerializer.Deserialize<Person>(json, Options);

        await Assert.That(student).IsTypeOf<Student>();
        await Assert.That(((Student)student!).Id).IsEqualTo("42");
    }

    [Test]
    public async Task DeserializeTeacher() {
        var json = """{"type":"teacher","subject":"Math"}""";
        var teacher = JsonSerializer.Deserialize<Person>(json, Options);

        await Assert.That(teacher).IsTypeOf<Teacher>();
        await Assert.That(((Teacher)teacher!).Subject).IsEqualTo("Math");
    }

    [Test]
    public async Task SerializeStudent() {
        var student = new Student("42");
        var json = JsonSerializer.Serialize(student, Options);

        await Assert.That(json).IsEqualTo("""{"id":"42","type":"student"}""");
    }

    #endregion

    enum PlaceType
    {
        [JsonStringEnumMemberName("school")] School,
        [JsonStringEnumMemberName("home")] Home
    }

    [RzJsonDerivedType(PlaceType.Home)]
    record Place(PlaceType Type);

    [RzJsonDerivedType(PlaceType.School)]
    record School(string Name, Person[] People) : Place(PlaceType.School);

    #region Base class cases

    [Test]
    public async Task DeserializeSchool() {
        var json = """{"type":"school","name":"RZ","people":[{"type":"student","id":"42"},{"type":"teacher","subject":"Math"}]}""";
        var school = JsonSerializer.Deserialize<Place>(json, Options);

        await Assert.That(school).IsTypeOf<School>();
        await Assert.That(school).IsEquivalentTo(new School("RZ", [new Student("42"), new Teacher("Math")]));
    }

    [Test]
    [DisplayName("Serializing a base class is not supported!")]
    public async Task SerializePlace() {
        var place = new Place(PlaceType.Home);

        Action action = () => JsonSerializer.Serialize(place, Options);

        await Assert.That(action).Throws<JsonException>().WithMessage("Serializing a base class is not supported!");
    }

    [Test]
    [DisplayName("Deserializing a base class is not supported!")]
    public async Task DeserializeBaseClass() {
        var json = """{"type":"home"}""";
        Action action = () => JsonSerializer.Deserialize<Place>(json, Options);

        await Assert.That(action).Throws<JsonException>().WithMessage("Deserializing a base class is not supported!");
    }

    #endregion

    #region Multiple parents

    abstract record Officer(PersonType Type) : Person(Type);

    [RzJsonDerivedType(PersonType.Accountant)]
    sealed record Accountant(string Grade) : Officer(PersonType.Accountant);

    [Test]
    [DisplayName("Deserialize Accountant to Person")]
    public async Task DeserializeAccountantToPerson() {
        var json = """{"type":"accountant","grade":"A"}""";
        var accountant = JsonSerializer.Deserialize<Person>(json, Options);

        await Assert.That(accountant).IsTypeOf<Accountant>();
        await Assert.That(((Accountant)accountant!).Grade).IsEqualTo("A");
    }

    [Test]
    [DisplayName("Deserialize Accountant to Officer")]
    public async Task DeserializeAccountantToOfficer() {
        var json = """{"type":"accountant","grade":"A"}""";
        var accountant = JsonSerializer.Deserialize<Officer>(json, Options);

        await Assert.That(accountant).IsTypeOf<Accountant>();
        await Assert.That(((Accountant)accountant!).Grade).IsEqualTo("A");
    }

    #endregion
}

#endif
