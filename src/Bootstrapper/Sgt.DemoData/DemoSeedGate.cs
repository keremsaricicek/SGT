using Sgt.BuildingBlocks;
using Sgt.Modules.CasinoPlayer.Application.Search;
using Sgt.Modules.Configuration.Application.Markers;

namespace Sgt.DemoData;

/// <summary>Kapıdan geçen yükleme: tenant kilidini tutar; bırakılınca kilit çözülür.</summary>
public sealed class DemoSeedPermit(IAsyncDisposable heldLock) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => heldLock.DisposeAsync();
}

/// <summary>
/// Yükleme öncesi denetim (Spec 012 KK5): tenant kilidi → tamamlanma işareti varsa yükleme yok (null) →
/// başlangıç işareti veya veri var ama tamamlanma yoksa yarım yükleme (reset önerilir) → saat ilk güne → başlangıç işareti
/// (tenant'a ilk yazım) → tenant ön koşulu (lisans + süper admin).
/// </summary>
public sealed class DemoSeedGate(DemoCommands commands, IAdvisoryLock locks, IDemoTenantBootstrap bootstrap, DemoClock clock)
{
    /// <summary>Yükleme başladı; tenant'a ilk yazılan kayıttır.</summary>
    public const string StartedMarker = "DEMO_DATA_STARTED_V1";

    private const string FirstPlayerNumber = "P000001";

    public static string LockName(TenantId tenant) => $"sgt:demo-seed:{tenant.Value}";

    /// <summary>Yükleme yapılacaksa kilitli izin, tamamlanmışsa null döner; yarım yüklemede veya kilit doluysa durur.</summary>
    public async Task<DemoSeedPermit?> Enter(DemoSeedOptions options, AuditContext audit, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(options);
        var held = await locks.TryAcquire(LockName(options.Tenant), ct)
            ?? throw new DemoSeedException($"Tenant {options.Tenant.Value} için başka bir demo yüklemesi sürüyor.");
        var permit = new DemoSeedPermit(held);
        try
        {
            if (await IsSet(options.Tenant, DemoCompletion.CompletedMarker, ct))
            {
                await permit.DisposeAsync();
                return null;
            }

            if (await IsSet(options.Tenant, StartedMarker, ct) || await HasPlayers(options.Tenant, ct))
            {
                throw new DemoSeedException(
                    $"Tenant {options.Tenant.Value} içinde yarım kalmış demo yüklemesi var; reset önerilir (build/demo.sh reset --yes).");
            }

            // Saat ilk günün sabahına alınır (yalnızca yükleme yapılacaksa; tamamlanmış tenant'ta saat geri alınmaz).
            clock.Set(options.Timeline.SetupStart);
            await commands.Do<ICommandHandler<SetTenantMarkerCommand>>(
                "başlangıç işareti", h => h.Handle(new SetTenantMarkerCommand(options.Tenant, StartedMarker, audit), ct));
            await bootstrap.Prepare(options.Tenant, DemoUsers.SuperAdmin.Id, ct);
            return permit;
        }
        catch
        {
            await permit.DisposeAsync();
            throw;
        }
    }

    private Task<bool> IsSet(TenantId tenant, string code, CancellationToken ct) =>
        commands.Query<IsTenantMarkerSetQuery, bool>(new IsTenantMarkerSetQuery(tenant, code), ct);

    // Başlangıç işareti öncesi sürümlerden kalmış veri için ek güvence.
    private async Task<bool> HasPlayers(TenantId tenant, CancellationToken ct) =>
        (await commands.Query<SearchPlayersQuery, IReadOnlyList<PlayerSummary>>(new SearchPlayersQuery(tenant, FirstPlayerNumber), ct)).Count > 0;
}
