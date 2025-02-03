﻿using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace RZ.Foundation.Json;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TypedClassConverterTest
{
    enum PersonType
    {
        [JsonStringEnumMemberName("student")] Student,
        [JsonStringEnumMemberName("teacher")] Teacher
    }

    abstract record Person(PersonType Type);

    [JsonDerivedType(PersonType.Student)]
    sealed record Student(string Id) : Person(PersonType.Student);

    [JsonDerivedType(PersonType.Teacher)]
    sealed record Teacher(string Subject) : Person(PersonType.Teacher);

    static readonly JsonSerializerOptions Options = new JsonSerializerOptions {
        Converters = { new TypedClassConverter([typeof(Person).Assembly]) }
    }.UseRzRecommendedSettings();

    [Fact]
    public void DeserializeStudent() {
        var json = """{"type":"student","id":"42"}""";
        var student = JsonSerializer.Deserialize<Person>(json, Options);

        student.Should().BeOfType<Student>();
        student.As<Student>().Id.Should().Be("42", $"but {student}");
    }

    [Fact]
    public void DeserializeTeacher() {
        var json = "{\"type\":\"teacher\",\"subject\":\"Math\"}";
        var teacher = JsonSerializer.Deserialize<Person>(json, Options);

        teacher.Should().BeOfType<Teacher>();
        teacher.As<Teacher>().Subject.Should().Be("Math");
    }

    [Fact]
    public void SerializeStudent() {
        var student = new Student("42");
        var json = JsonSerializer.Serialize(student, Options);

        json.Should().Be("""{"id":"42","type":"student"}""");
    }

    enum PlaceType
    {
        [JsonStringEnumMemberName("school")] School,
        [JsonStringEnumMemberName("home")] Home
    }

    [JsonDerivedType(PlaceType.Home)]
    record Place(PlaceType Type);

    [JsonDerivedType(PlaceType.School)]
    record School(string Name, Person[] People) : Place(PlaceType.School);

    [Fact]
    public void DeserializeSchool() {
        var json = """{"type":"school","name":"RZ","people":[{"type":"student","id":"42"},{"type":"teacher","subject":"Math"}]}""";
        var school = JsonSerializer.Deserialize<Place>(json, Options);

        school.Should().BeOfType<School>();
        school.Should().BeEquivalentTo(new School("RZ", [new Student("42"), new Teacher("Math")]));
    }

    [Fact(DisplayName = "Serializing a base class is not supported!")]
    public void SerializePlace() {
        var place = new Place(PlaceType.Home);

        var action = () => JsonSerializer.Serialize(place, Options);

        action.Should().Throw<JsonException>().WithMessage("Serializing a base class is not supported!");
    }

    [Fact(DisplayName = "Deserializing a base class is not supported!")]
    public void DeserializeBaseClass() {
        var json = """{"type":"home"}""";
        var action = () => JsonSerializer.Deserialize<Place>(json, Options);

        action.Should().Throw<JsonException>().WithMessage("Deserializing a base class is not supported!");
    }
}