namespace RZ.Foundation;

public class TryTest
{
    [Test]
    [DisplayName("Capture error from Task<T>")]
    public async Task TryTaskValue() {
        var (error, result) = await Try(JustThrow());

        await Assert.That(error).IsTypeOf<Exception>();
        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    [DisplayName("Capture result from Task<T>")]
    public async Task TryTaskValueSuccess() {
        var (error, result) = await Try(JustReturn());

        await Assert.That(error).IsNull();
        await Assert.That(result).IsEqualTo(123);
    }

    static async ValueTask<int> JustThrow()  => throw new Exception("JustThrow");
    static       ValueTask<int> JustReturn() => new(123);
}