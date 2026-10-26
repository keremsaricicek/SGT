namespace Sgt.Windows.Core.Shared;

/// <summary>Bir kullanıcı işleminin kimlikleri: TransactionId (idempotency anahtarı) ve CorrelationId (iz).</summary>
public sealed record OperationKey(Guid TransactionId, Guid CorrelationId);

/// <summary>
/// Bir kullanıcı işlemi için kimliği bir kez üretir; aynı girdilerle tekrar denenirse aynı kimliği verir.
/// Böylece ağ hatasından sonra "tekrar dene" ikinci bir para hareketi oluşturmaz (sunucu aynı anahtarı tanır).
/// Girdi değişirse yeni işlem sayılır; başarıdan sonra <see cref="Complete"/> çağrılır.
/// </summary>
public sealed class PendingOperation(IIdGenerator ids)
{
    private object? _fingerprint;

    public OperationKey? Current { get; private set; }

    public OperationKey For(object fingerprint)
    {
        ArgumentNullException.ThrowIfNull(fingerprint);
        if (Current is null || !Equals(_fingerprint, fingerprint))
        {
            Current = new OperationKey(ids.NewId(), ids.NewId());
            _fingerprint = fingerprint;
        }

        return Current;
    }

    public void Complete()
    {
        Current = null;
        _fingerprint = null;
    }
}
