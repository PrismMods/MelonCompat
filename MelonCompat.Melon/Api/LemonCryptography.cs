using System.IO;
using System.Security.Cryptography;

namespace MelonLoader.Lemons.Cryptography;

// MelonLoader ships its own hash implementations to avoid depending on the
// platform's crypto stack. Here they are the platform's crypto stack.
public class LemonMD5 {
    public byte[] ComputeHash(byte[] data) => ComputeMD5Hash(data);
    public byte[] ComputeHash(byte[] data, int offset, int count) => ComputeMD5Hash(data, offset, count);
    public byte[] ComputeHash(Stream stream) => ComputeMD5Hash(stream);

    public static byte[] ComputeMD5Hash(byte[] data) => Hash(MD5.Create(), data, 0, data?.Length ?? 0);
    public static byte[] ComputeMD5Hash(byte[] data, int offset, int count) => Hash(MD5.Create(), data, offset, count);
    public static byte[] ComputeMD5Hash(Stream stream) => Hash(MD5.Create(), stream);

    internal static byte[] Hash(HashAlgorithm algorithm, byte[] data, int offset, int count) {
        using(algorithm) return algorithm.ComputeHash(data ?? [], offset, count);
    }

    internal static byte[] Hash(HashAlgorithm algorithm, Stream stream) {
        using(algorithm) return algorithm.ComputeHash(stream);
    }
}

public class LemonSHA256 {
    public byte[] ComputeHash(byte[] data) => ComputeSHA256Hash(data);
    public byte[] ComputeHash(byte[] data, int offset, int count) => ComputeSHA256Hash(data, offset, count);
    public byte[] ComputeHash(Stream stream) => ComputeSHA256Hash(stream);

    public static byte[] ComputeSHA256Hash(byte[] data) => LemonMD5.Hash(SHA256.Create(), data, 0, data?.Length ?? 0);
    public static byte[] ComputeSHA256Hash(byte[] data, int offset, int count) => LemonMD5.Hash(SHA256.Create(), data, offset, count);
    public static byte[] ComputeSHA256Hash(Stream stream) => LemonMD5.Hash(SHA256.Create(), stream);
}

public class LemonSHA512 {
    public byte[] ComputeHash(byte[] data) => ComputeSHA512Hash(data);
    public byte[] ComputeHash(byte[] data, int offset, int count) => ComputeSHA512Hash(data, offset, count);
    public byte[] ComputeHash(Stream stream) => ComputeSHA512Hash(stream);

    public static byte[] ComputeSHA512Hash(byte[] data) => LemonMD5.Hash(SHA512.Create(), data, 0, data?.Length ?? 0);
    public static byte[] ComputeSHA512Hash(byte[] data, int offset, int count) => LemonMD5.Hash(SHA512.Create(), data, offset, count);
    public static byte[] ComputeSHA512Hash(Stream stream) => LemonMD5.Hash(SHA512.Create(), stream);
}
