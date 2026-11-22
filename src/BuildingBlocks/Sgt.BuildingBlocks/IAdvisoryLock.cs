namespace Sgt.BuildingBlocks;

/// <summary>
/// Süreçler arası, ada göre özel kilit (ör. aynı tenant'a iki demo yüklemesi aynı anda çalışmasın).
/// Kilit alınamazsa null döner; dönen nesne bırakılınca kilit çözülür.
/// </summary>
public interface IAdvisoryLock
{
    Task<IAsyncDisposable?> TryAcquire(string name, CancellationToken ct);
}
