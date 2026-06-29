using System.Security.Cryptography;
using System.Text;

namespace RZ.Foundation.Helpers;

[PublicAPI]
public static class Encryption
{
    public const string TAMPER_ERROR = "tampered";

    const int NONCE_LENGTH = 16;

    const int PBKDF2_ITERATIONS = 600_000; // OWASP-recommended minimum for PBKDF2-HMAC-SHA256
    const int WEAK_MIN_LENGTH = 8;
    const int STRONG_MIN_LENGTH = 16;

    // Fixed, library-wide derivation constants. A per-key salt is impossible here because these functions must
    // be deterministic; these provide domain separation only, NOT anti-precomputation. Do NOT change once
    // shipped — changing them changes every previously derived key.
    static readonly byte[] AesPbkdf2Salt =                 // "RZFndnKDFv1salt!" (ASCII)
        [0x52, 0x5A, 0x46, 0x6E, 0x64, 0x6E, 0x4B, 0x44, 0x46, 0x76, 0x31, 0x73, 0x61, 0x6C, 0x74, 0x21];
    static readonly byte[] AesHkdfInfo =                   // HKDF context string for domain separation
        "RZ.Foundation/AES-HKDF/v1"u8.ToArray();

    /// <summary>
    /// Generate a Nonce from a string. The Nonce will be 16 bytes long.
    /// </summary>
    /// <param name="s">A string for making a nonce. It shouldn't be longer than 16 characters.</param>
    /// <returns>A nonce</returns>
    [Obsolete("Use Encrypt(byte[], byte[]) instead")]
    public static byte[] NonceFromAscii(string s)
        => Enumerable.Repeat(Encoding.ASCII.GetBytes(s), NONCE_LENGTH).SelectMany(x => x).Take(NONCE_LENGTH).ToArray();

    /// <summary>
    /// Generate 256-bit random AES key.
    /// </summary>
    /// <returns></returns>
    public static byte[] RandomAesKey()
        => RandomNumberGenerator.GetBytes(32);

    static Outcome<byte[]> DeriveAesKey(string key, int n, int minLength, Func<byte[]> derive) {
        if (n is not (16 or 24 or 32))
            return ErrorInfo.New(INVALID_REQUEST, $"AES key size must be 16, 24 or 32 bytes (got {n})");
        if (string.IsNullOrEmpty(key) || key.Length < minLength)
            return ErrorInfo.New(INVALID_REQUEST, $"Key string must be at least {minLength} characters");
        return derive();
    }

    /// <summary>
    /// Deterministically derives an AES key of <paramref name="n"/> bytes from a <b>weak / human passphrase</b>
    /// using PBKDF2-HMAC-SHA256 (600,000 iterations) with a fixed library salt. The same <paramref name="key"/>
    /// and <paramref name="n"/> always produce the same bytes, so the key can be re-derived instead of stored.
    /// </summary>
    /// <param name="key">The passphrase. Must be at least 8 characters.</param>
    /// <param name="n">Key length in bytes: 16, 24 or 32 (AES-128/192/256). Defaults to 32.</param>
    /// <returns>
    /// Exactly <paramref name="n"/> key bytes, or an <see cref="ErrorInfo"/> with code <c>INVALID_REQUEST</c>
    /// when <paramref name="n"/> is not 16/24/32, or <paramref name="key"/> is null/empty or shorter than 8 characters.
    /// </returns>
    /// <remarks>
    /// Strength is bounded by the entropy of <paramref name="key"/>; PBKDF2 only raises the cost per brute-force
    /// guess, it cannot add missing entropy. The salt is fixed (a per-key salt would break determinism), giving
    /// domain separation only. This call is intentionally slow — derive once and cache. For input that is already
    /// high-entropy use <see cref="CreateAesKeyFromStrongText"/>; for a non-reproducible random key use
    /// <see cref="RandomAesKey"/>.
    /// </remarks>
    public static Outcome<byte[]> CreateAesKeyFromWeakText(string key, int n = 32)
        => DeriveAesKey(key, n, WEAK_MIN_LENGTH,
                        () => Rfc2898DeriveBytes.Pbkdf2(key, AesPbkdf2Salt, PBKDF2_ITERATIONS, HashAlgorithmName.SHA256, outputLength: n));

    /// <summary>
    /// Deterministically derives an AES key of <paramref name="n"/> bytes from <b>already high-entropy</b> keying
    /// material (a generated secret, base64/hex key, etc.) using HKDF-SHA256. Fast, but performs no key stretching.
    /// </summary>
    /// <param name="key">High-entropy source text. Must be at least 16 characters.</param>
    /// <param name="n">Key length in bytes: 16, 24 or 32 (AES-128/192/256). Defaults to 32.</param>
    /// <returns>
    /// Exactly <paramref name="n"/> key bytes, or an <see cref="ErrorInfo"/> with code <c>INVALID_REQUEST</c>
    /// when <paramref name="n"/> is not 16/24/32, or <paramref name="key"/> is null/empty or shorter than 16 characters.
    /// </returns>
    /// <remarks>
    /// HKDF assumes the input is already strong; it does NOT protect a weak passphrase. If the input is a human
    /// passphrase, use <see cref="CreateAesKeyFromWeakText"/> instead. A fixed HKDF <c>info</c> context provides
    /// domain separation only.
    /// </remarks>
    public static Outcome<byte[]> CreateAesKeyFromStrongText(string key, int n = 32)
        => DeriveAesKey(key, n, STRONG_MIN_LENGTH,
                        () => HKDF.DeriveKey(HashAlgorithmName.SHA256, Encoding.UTF8.GetBytes(key), n, salt: null, info: AesHkdfInfo));

    [Obsolete("Use Encrypt(byte[], byte[]) instead")]
    public static Aes CreateAes(byte[] key, byte[] nonce) {
        var aes = Aes.Create();
        aes.Key = key;
        aes.IV = nonce;
        return aes;
    }

    extension(Aes aes)
    {
        [Obsolete("Use Encrypt(byte[], byte[]) instead")]
        public byte[] Encrypt(byte[] data) {
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream(data.Length);
            using (var encryptStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                encryptStream.Write(data);

            return ms.ToArray();
        }

        [Obsolete("Use Decrypt(byte[], byte[]) instead")]
        public byte[] Decrypt(byte[] encrypted) {
            using var output = new MemoryStream(encrypted.Length);
            using var decryptor = aes.CreateDecryptor();
            using var msd = new MemoryStream(encrypted);
            using var decryptStream = new CryptoStream(msd, decryptor, CryptoStreamMode.Read);
            int read;
            while ((read = decryptStream.ReadByte()) != -1)
                output.WriteByte((byte)read);

            return output.ToArray();
        }
    }

    /// <summary>
    /// Encrypts <paramref name="data"/> with AES-GCM (authenticated encryption) using <paramref name="key"/>.
    /// A fresh random 12-byte nonce is generated on every call, so encrypting the same data twice produces
    /// different output.
    /// </summary>
    /// <param name="key">
    /// The secret key. Must be a valid AES key size — 16, 24 or 32 bytes (AES-128/192/256); a 256-bit key is
    /// recommended (see <see cref="RandomAesKey"/>).
    /// </param>
    /// <param name="data">The plaintext to encrypt.</param>
    /// <returns>
    /// On success, the self-describing payload laid out as <c>[nonce (12 bytes)][tag (16 bytes)][ciphertext]</c>,
    /// ready to pass back to <see cref="Decrypt(byte[],byte[])"/>. An empty <paramref name="data"/> returns an empty
    /// array unchanged (note: an empty payload carries no authentication tag).
    /// On failure, an <see cref="ErrorInfo"/> with code <c>INVALID_REQUEST</c> (empty or wrong-sized key) or
    /// <c>NOT_SUPPORTED</c> (the platform has no AES-GCM implementation).
    /// </returns>
    public static Outcome<byte[]> Encrypt(byte[] key, byte[] data) {
        if (key.Length == 0) return ErrorInfo.New(INVALID_REQUEST, "Key must not be empty");
        if (data.Length == 0) return data;

        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        var ciphertext = new byte[data.Length];
        if (Fail(CreateGcm(key, tag.Length), out var e, out var aes)) return e.Trace();

        using(aes){
            aes.Encrypt(nonce, data, ciphertext, tag);
            // layout: [nonce][tag][ciphertext]
            byte[] result = [..nonce, ..tag, ..ciphertext];
            return result;
        }
    }

    /// <summary>
    /// Verifies and decrypts a payload produced by <see cref="Encrypt(byte[],byte[])"/> using AES-GCM.
    /// The authentication tag is checked before any plaintext is returned, so tampering — or a wrong key —
    /// fails with an error instead of yielding corrupt data.
    /// </summary>
    /// <param name="key">The secret key used to encrypt the payload (16, 24 or 32 bytes).</param>
    /// <param name="payload">
    /// The encrypted payload as produced by <see cref="Encrypt(byte[],byte[])"/>, laid out as
    /// <c>[nonce (12 bytes)][tag (16 bytes)][ciphertext]</c>.
    /// </param>
    /// <returns>
    /// On success, the original plaintext (an empty <paramref name="payload"/> returns an empty array unchanged).
    /// On failure, an <see cref="ErrorInfo"/> whose code is one of:
    /// <list type="bullet">
    ///   <item><description><see cref="TAMPER_ERROR"/> — the tag did not validate: the payload was tampered with, or the key is wrong.</description></item>
    ///   <item><description><c>INVALID_REQUEST</c> — empty key, a wrong-sized key, or a payload shorter than nonce + tag (28 bytes).</description></item>
    ///   <item><description><c>NOT_SUPPORTED</c> — the platform has no AES-GCM implementation.</description></item>
    /// </list>
    /// </returns>
    public static Outcome<byte[]> Decrypt(byte[] key, byte[] payload) {
        if (key.Length == 0) return ErrorInfo.New(INVALID_REQUEST, "Key must not be empty");
        if (payload.Length == 0) return payload;

        var nLen = AesGcm.NonceByteSizes.MaxSize;
        var tLen = AesGcm.TagByteSizes.MaxSize;
        if (payload.Length < nLen + tLen) return ErrorInfo.New(INVALID_REQUEST, "Payload is too short");

        var nonce = payload.AsSpan(0, nLen);
        var tag = payload.AsSpan(nLen, tLen);
        var ciphertext = payload.AsSpan(nLen + tLen);
        var plain = new byte[ciphertext.Length];
        if (Fail(CreateGcm(key, tag.Length), out var e, out var aes)) return e.Trace();
        using (aes)
            try{
                aes.Decrypt(nonce, ciphertext, tag, plain); // throws CryptographicException on tamper
            }
            catch (CryptographicException ex){
                return ErrorInfo.New(TAMPER_ERROR, "Payload is tampered", innerError: ErrorFrom.Exception((Exception)ex));
            }
            catch (Exception ex){
                return ErrorFrom.Exception(ex);
            }
        return plain;
    }

    static Outcome<AesGcm> CreateGcm(byte[] key, int tLen) {
        try{
            return new AesGcm(key, tLen);
        }
        catch (PlatformNotSupportedException){
            return ErrorInfo.New(NOT_SUPPORTED, "Platform does not support GCM encryption");
        }
        catch (CryptographicException e){
            return ErrorInfo.New(INVALID_REQUEST, $"Specified key is not a valid size ({key.Length})", innerError: ErrorFrom.Exception((Exception)e));
        }
        catch (Exception e){
            return ErrorFrom.Exception(e);
        }
    }
}