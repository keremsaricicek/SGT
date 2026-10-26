using System.Security.Cryptography;
using System.Text;

namespace Sgt.BuildingBlocks.Infrastructure.Security;

/// <summary>
/// AES-256-GCM şifreleme ve HMAC-SHA256 kör indeks (ADR-0005).
/// Şifreli metin biçimi: base64(nonce[12] | tag[16] | ciphertext).
/// </summary>
public sealed class AesGcmPersonalDataProtector : IPersonalDataProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _encryptionKey;
    private readonly byte[] _indexKey;

    public AesGcmPersonalDataProtector(byte[] encryptionKey, byte[] indexKey)
    {
        ArgumentNullException.ThrowIfNull(encryptionKey);
        ArgumentNullException.ThrowIfNull(indexKey);
        if (encryptionKey.Length != 32 || indexKey.Length != 32)
        {
            throw new ArgumentException("Anahtarlar 32 bayt olmalıdır.");
        }

        _encryptionKey = encryptionKey;
        _indexKey = indexKey;
    }

    public string Protect(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        var data = Encoding.UTF8.GetBytes(plaintext);
        var output = new byte[NonceSize + TagSize + data.Length];
        var nonce = output.AsSpan(0, NonceSize);
        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Encrypt(nonce, data, output.AsSpan(NonceSize + TagSize), output.AsSpan(NonceSize, TagSize));
        return Convert.ToBase64String(output);
    }

    public string Unprotect(string ciphertext)
    {
        var input = Convert.FromBase64String(ciphertext);
        var data = new byte[input.Length - NonceSize - TagSize];
        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Decrypt(input.AsSpan(0, NonceSize), input.AsSpan(NonceSize + TagSize), input.AsSpan(NonceSize, TagSize), data);
        return Encoding.UTF8.GetString(data);
    }

    public string BlindIndex(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Convert.ToBase64String(HMACSHA256.HashData(_indexKey, Encoding.UTF8.GetBytes(value)));
    }

    /// <summary>Yapılandırmadaki base64 anahtarlardan oluşturur.</summary>
    public static AesGcmPersonalDataProtector FromBase64(string? encryptionKey, string? indexKey) =>
        new(
            Convert.FromBase64String(encryptionKey ?? throw new InvalidOperationException("Security:PersonalDataKey tanımlı değil.")),
            Convert.FromBase64String(indexKey ?? throw new InvalidOperationException("Security:BlindIndexKey tanımlı değil.")));
}
