using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace RZ.Foundation.Types;

public class ErrorInfoTest
{
    [Fact]
    public void SerializeErrorInfo() {
        var error = new ErrorInfo("dummy-code");

        var json = JsonSerializer.Serialize(error);

        json.Should().Be("""{"Code":"dummy-code","Message":"dummy-code"}""");
    }

    [Fact]
    public void DeserializeErrorInfo() {
        var json = """{"Code":"dummy-code","Message":"dummy-code"}""";

        var error = JsonSerializer.Deserialize<ErrorInfo>(json);

        error.Should().Be(new ErrorInfo("dummy-code"));
    }
}