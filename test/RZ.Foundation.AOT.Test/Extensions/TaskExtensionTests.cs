namespace RZ.Foundation.Extensions;

public sealed class TaskExtensionTests
{
    [Test]
    [DisplayName("Task not error, OnError just returns")]
    public async Task OnError_WithSuccess() {
        var x = Task.FromResult(123);

        var y = await On(x).Catch(_ => -1);

        await Assert.That(y).IsEqualTo(123);
    }

    [Test]
    [DisplayName("Task error, OnError returns -1")]
    public async Task OnError_WithException() {
        var x = Task.FromException<int>(new Exception("Test"));

        var y = await On(x).Catch(_ => -1);

        await Assert.That(y).IsEqualTo(-1);
    }

    [Test]
    [DisplayName("Task error, do an effect")]
    public async Task OnError_EffectBeforeThrow() {
        var x = Task.FromException<int>(new Exception("Test"));

        var effect = false;

        await Assert.That(() => On(x).BeforeThrow(_ => effect = true)).Throws<Exception>();
        await Assert.That(effect).IsTrue();
    }
}