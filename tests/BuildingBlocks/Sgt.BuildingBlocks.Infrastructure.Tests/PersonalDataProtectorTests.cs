using System.Security.Cryptography;
using Sgt.BuildingBlocks.Infrastructure.Security;

namespace Sgt.BuildingBlocks.Infrastructure.Tests;

public sealed class PersonalDataProtectorTests
{
    private readonly AesGcmPersonalDataProtector _protector =
        new(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(32));

    [Fact]
    public void Protect_round_trips_and_does_not_leak_plaintext()
    {
        var cipher = _protector.Protect("12345678901");

        Assert.DoesNotContain("12345678901", cipher, StringComparison.Ordinal);
        Assert.Equal("12345678901", _protector.Unprotect(cipher));
        Assert.NotEqual(cipher, _protector.Protect("12345678901"));
    }

    [Fact]
    public void Tampered_ciphertext_is_rejected()
    {
        var bytes = Convert.FromBase64String(_protector.Protect("gizli"));
        bytes[^1] ^= 0xFF;

        Assert.ThrowsAny<CryptographicException>(() => _protector.Unprotect(Convert.ToBase64String(bytes)));
    }

    [Fact]
    public void Blind_index_is_deterministic_and_key_dependent()
    {
        var other = new AesGcmPersonalDataProtector(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(32));

        Assert.Equal(_protector.BlindIndex("A1"), _protector.BlindIndex("A1"));
        Assert.NotEqual(_protector.BlindIndex("A1"), _protector.BlindIndex("A2"));
        Assert.NotEqual(_protector.BlindIndex("A1"), other.BlindIndex("A1"));
    }
}
