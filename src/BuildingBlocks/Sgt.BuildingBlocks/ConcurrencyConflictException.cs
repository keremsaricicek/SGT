namespace Sgt.BuildingBlocks;

/// <summary>
/// Kayıt, okunduktan sonra başka biri tarafından değiştirildi. İşlem kaydedilmedi; kullanıcı güncel veriyle
/// yeniden denemelidir. API bunu 409 Conflict olarak döner.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("Kayıt başka bir kullanıcı tarafından değiştirildi; güncel veriyle yeniden deneyin.")
    {
    }

    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
