namespace RZ.Foundation.Helpers;

public sealed class EncryptionTest
{
    // 28 chars — valid for both the weak (>= 8) and strong (>= 16) floors.
    const string GoodKey = "correct horse battery staple";

    #region Deterministic

    [Test]
    public async ValueTask WeakText_IsDeterministic() {
        var a = Encryption.CreateAesKeyFromWeakText(GoodKey).Unwrap();
        var b = Encryption.CreateAesKeyFromWeakText(GoodKey).Unwrap();
        await Assert.That(a.SequenceEqual(b)).IsTrue();
    }

    [Test]
    public async ValueTask StrongText_IsDeterministic() {
        var a = Encryption.CreateAesKeyFromStrongText(GoodKey).Unwrap();
        var b = Encryption.CreateAesKeyFromStrongText(GoodKey).Unwrap();
        await Assert.That(a.SequenceEqual(b)).IsTrue();
    }

    #endregion

    #region Length

    [Test]
    public async ValueTask WeakText_DefaultLengthIs32() =>
        await Assert.That(Encryption.CreateAesKeyFromWeakText(GoodKey).Unwrap().Length).IsEqualTo(32);

    [Test]
    public async ValueTask StrongText_DefaultLengthIs32() =>
        await Assert.That(Encryption.CreateAesKeyFromStrongText(GoodKey).Unwrap().Length).IsEqualTo(32);

    [Test]
    public async ValueTask WeakText_Length16() =>
        await Assert.That(Encryption.CreateAesKeyFromWeakText(GoodKey, 16).Unwrap().Length).IsEqualTo(16);

    [Test]
    public async ValueTask WeakText_Length24() =>
        await Assert.That(Encryption.CreateAesKeyFromWeakText(GoodKey, 24).Unwrap().Length).IsEqualTo(24);

    [Test]
    public async ValueTask StrongText_Length16() =>
        await Assert.That(Encryption.CreateAesKeyFromStrongText(GoodKey, 16).Unwrap().Length).IsEqualTo(16);

    #endregion

    #region Distinctness

    [Test]
    public async ValueTask WeakText_DifferentInputs_DifferentKeys() {
        var a = Encryption.CreateAesKeyFromWeakText(GoodKey).Unwrap();
        var b = Encryption.CreateAesKeyFromWeakText(GoodKey + "!").Unwrap();
        await Assert.That(a.SequenceEqual(b)).IsFalse();
    }

    [Test]
    public async ValueTask StrongText_DifferentInputs_DifferentKeys() {
        var a = Encryption.CreateAesKeyFromStrongText(GoodKey).Unwrap();
        var b = Encryption.CreateAesKeyFromStrongText(GoodKey + "!").Unwrap();
        await Assert.That(a.SequenceEqual(b)).IsFalse();
    }

    [Test]
    public async ValueTask WeakAndStrong_SameInput_ProduceDifferentKeys() {
        // Different algorithms + domain separation: the two paths must not be interchangeable.
        var weak = Encryption.CreateAesKeyFromWeakText(GoodKey).Unwrap();
        var strong = Encryption.CreateAesKeyFromStrongText(GoodKey).Unwrap();
        await Assert.That(weak.SequenceEqual(strong)).IsFalse();
    }

    #endregion

    #region Round-trip

    [Test]
    public async ValueTask DerivedKey_RoundTripsThroughEncryptDecrypt() {
        var key = Encryption.CreateAesKeyFromWeakText(GoodKey).Unwrap();
        byte[] plaintext = [1, 2, 3, 4, 5, 6, 7, 8];

        var payload = Encryption.Encrypt(key, plaintext).Unwrap();
        var recovered = Encryption.Decrypt(key, payload).Unwrap();

        await Assert.That(recovered.SequenceEqual(plaintext)).IsTrue();
    }

    #endregion

    #region Validation

    [Test]
    public async ValueTask WeakText_InvalidSize_FailsInvalidRequest() {
        var result = Encryption.CreateAesKeyFromWeakText(GoodKey, 20);
        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is(StandardErrorCodes.INVALID_REQUEST)).IsTrue();
    }

    [Test]
    public async ValueTask StrongText_InvalidSize_FailsInvalidRequest() {
        var result = Encryption.CreateAesKeyFromStrongText(GoodKey, 15);
        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is(StandardErrorCodes.INVALID_REQUEST)).IsTrue();
    }

    [Test]
    public async ValueTask WeakText_TooShortKey_FailsInvalidRequest() {
        var result = Encryption.CreateAesKeyFromWeakText("short"); // 5 chars < 8
        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is(StandardErrorCodes.INVALID_REQUEST)).IsTrue();
    }

    [Test]
    public async ValueTask StrongText_TooShortKey_FailsInvalidRequest() {
        var result = Encryption.CreateAesKeyFromStrongText("abcdefghij"); // 10 chars < 16
        await Assert.That(result.IsFail).IsTrue();
        await Assert.That(result.UnwrapError().Is(StandardErrorCodes.INVALID_REQUEST)).IsTrue();
    }

    [Test]
    public async ValueTask TenCharKey_AcceptedByWeak_RejectedByStrong() {
        const string tenChars = "abcdefghij"; // 10: >= weak floor (8), < strong floor (16)
        await Assert.That(Encryption.CreateAesKeyFromWeakText(tenChars).IsSuccess).IsTrue();
        await Assert.That(Encryption.CreateAesKeyFromStrongText(tenChars).IsFail).IsTrue();
    }

    #endregion
}
