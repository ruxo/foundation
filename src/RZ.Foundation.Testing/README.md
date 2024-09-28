# RZ.Foundation.Testing

This library provides a set of utilities to help with testing.

## XUnit Helpers

### `DebugTo`

Redirect `Debug.WriteLine` output to the test output.

```csharp
public class SampleTest(ITestOutputHelper output)
{
    [Fact]
    public void Test()
    {
        using _ = DebugTo.XUnit(output);
        // test code.. all Debug.WriteLine output will be redirected to the test output
    }
}
```