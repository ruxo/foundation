using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace RZ.Foundation;

[PublicAPI]
public static class Encryption
{
	const int NonceLength = 16;

	/// <summary>
	/// Generate a Nonce from a string. The Nonce will be 16 bytes long.
	/// </summary>
	/// <param name="s">A string for making a nonce. It shouldn't be longer than 16 characters.</param>
	/// <returns>A nonce</returns>
	public static byte[] NonceFromASCII(string s)
		=> Enumerable.Repeat(Encoding.ASCII.GetBytes(s), NonceLength).SelectMany(x => x).Take(NonceLength).ToArray();

	/// <summary>
	/// Generate 256-bit random AES key.
	/// </summary>
	/// <returns></returns>
	public static byte[] RandomAesKey()
		=> RandomNumberGenerator.GetBytes(32);

	public static Aes CreateAes(byte[] key, byte[] nonce) {
		var aes = Aes.Create();
		aes.Key = key;
		aes.IV = nonce;
		return aes;
	}

	public static byte[] Encrypt(this Aes aes, byte[] data) {
		using var encryptor = aes.CreateEncryptor();
		using var ms = new MemoryStream(data.Length);
		using (var encryptStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
			encryptStream.Write(data);

		return ms.ToArray();
	}

	public static byte[] Decrypt(this Aes aes, byte[] encrypted)
    {
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