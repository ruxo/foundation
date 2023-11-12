using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Json;

public sealed class SnakeCaseNamingPolicyTest
{
    [Fact]
    public void Serialization() {
        var option = new JsonSerializerOptions{ PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance };
        
        var result = JsonSerializer.Serialize(new{ FirstName = "John", Age = 123 }, option);

        result.Should().Be(@"{""first_name"":""John"",""age"":123}");
    }
    
    [Fact]
    public void Deserialization() {
        var option = new JsonSerializerOptions{ PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance };
        
        var result = JsonSerializer.Deserialize<Data>(@"{""first_name"":""John"",""age"":123}", option);

        result.Should().Be(new Data("John", 123));
    }

    readonly record struct Data(string FirstName, int Age);
}