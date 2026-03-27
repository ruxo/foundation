using JetBrains.Annotations;
using RZ.Foundation.Types;

namespace RZ.Foundation.Functional;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TryCatchTest
{
    [Test]
    public async ValueTask Try_catch_sync_outcome() {
        var outcome = SuccessOutcome(42);

        var result = TryCatch(() => outcome);

        await Assert.That(result).IsEqualTo(SuccessOutcome(42));
    }

    [Test]
    public async ValueTask Try_catch_async_outcome() {
        var outcome = new ValueTask<Outcome<int>>(SuccessOutcome(42));

        var result = await TryCatch(() => outcome);

        await Assert.That(result).IsEqualTo(SuccessOutcome(42));
    }

    [Test]
    public async ValueTask Try_catch_value() {
        var result = TryCatch(() => 42);

        await Assert.That(result).IsEqualTo(SuccessOutcome(42));
    }

    [Test]
    public async ValueTask Try_catch_async_value() {
        var result = await TryCatch(() => new ValueTask<int>(42));

        await Assert.That(result).IsEqualTo(SuccessOutcome(42));
    }

    [Test]
    public async ValueTask Try_catch_action() {
        var called = false;
        var result = TryCatch(() => {
                                  called = true;
                              });

        await Assert.That(result).IsEqualTo(SuccessOutcome(unit));
        await Assert.That(called).IsTrue();
    }

    [Test]
    public async ValueTask Try_catch_async_action() {
        var called = false;
        var result = await TryCatch(async () => {
            await Task.Yield();
            called = true;
        });

        await Assert.That(result).IsEqualTo(SuccessOutcome(unit));
        await Assert.That(called).IsTrue();
    }

    [Test]
    public async ValueTask Try_catch_action_exception() {
        var result = TryCatch(Test);

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is(StandardErrorCodes.Unhandled)).IsTrue();
        await Assert.That(result.UnwrapError().Message).IsEqualTo("test");
        return;

        void Test() {
            throw new Exception("test");
        }
    }

    [Test]
    public async ValueTask Try_catch_async_action_exception() {
        var result = await TryCatch(Test);

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is(StandardErrorCodes.Unhandled)).IsTrue();
        await Assert.That(result.UnwrapError().Message).IsEqualTo("test");
        return;

        async ValueTask Test() {
            await Task.Yield();
            throw new Exception("test");
        }
    }

    [Test]
    public async ValueTask Try_catch_errorinfoexception() {
        var result = TryCatch(Test);

        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is("test")).IsTrue();
        await Assert.That(result.UnwrapError().Message).IsEqualTo("message");
        return;

        void Test() {
            throw new ErrorInfoException("test", "message");
        }
    }
}