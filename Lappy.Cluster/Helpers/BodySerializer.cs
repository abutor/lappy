using Lappy.Cluster.Model;
using System.Security.Cryptography;

namespace Lappy.Cluster.Helpers;

record BodyItem(string Key, object? Value);

internal static class BodySerializer
{
    private readonly static byte[] _iv = Convert.FromBase64String("hwSPfkG6eP6VPCfQEZ7uxQ==");
    private readonly static byte[] _key = Convert.FromBase64String("g+jbGjT+2Jw/nDOcXOZmaG7CCTLuXEw9nKw/jgrmeoQ=");

    internal static string GetType(Type type) => $"{type.FullName}, {type.Assembly.FullName}";

    internal static BodyItem[]? Deserialize(byte[] body)
    {
        try
        {
            var input = (SerializedKey[])SpanJson.JsonSerializer.NonGeneric.Utf8.Deserialize(DecryptBytes(body), typeof(SerializedKey[]));
            return Array.ConvertAll(input ?? [], body => new BodyItem(body.Key, body.Data?.GetObject()));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return [];
        }

    }

    internal static byte[] Serialize(BodyItem[] input)
    {
        try
        {
            var array = Array.ConvertAll(input, body => new SerializedKey(body.Key, SerializedModel.Create(body.Value)));
            return EncryptBytes(SpanJson.JsonSerializer.NonGeneric.Utf8.Serialize(array));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return [];
        }
    }

    private static byte[] EncryptBytes(byte[] data)
    {
        using var aes = CreateAes();
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        return PerformCryptography(data, encryptor);
    }

    private static byte[] DecryptBytes(byte[] data)
    {
        using var aes = CreateAes();
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        return PerformCryptography(data, decryptor);
    }

    private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
    {
        using var ms = new MemoryStream();
        using var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        return ms.ToArray();
    }
    private static Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.KeySize = 128;
        aes.BlockSize = 128;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.ISO10126;
        aes.Key = _key;
        aes.IV = _iv;
        return aes;
    }
}
