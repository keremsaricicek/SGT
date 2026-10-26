namespace Sgt.BuildingBlocks;

/// <summary>
/// Kişisel verinin korunması (ADR-0005): şifreleme ve şifre çözmeden arama için kör indeks.
/// Uygulaması altyapıdadır; anahtarlar secret manager'dan gelir.
/// </summary>
public interface IPersonalDataProtector
{
    string Protect(string plaintext);

    string Unprotect(string ciphertext);

    /// <summary>Aynı girdi her zaman aynı değeri verir; değerden girdi bulunamaz.</summary>
    string BlindIndex(string value);
}
