using Sgt.BuildingBlocks;
using Sgt.Modules.GamingFloor.Application.Ports;
using Sgt.Modules.GamingFloor.Domain;
using Sgt.Modules.GamingFloor.Domain.Slots;

namespace Sgt.Modules.GamingFloor.Application.Slots;

/// <summary>Kaynaktan bağımsız tek okuma girdisi (elle, CSV, NEXIO, S Slot).</summary>
public sealed record MeterReadingInput(
    string AssetNumber, DateTimeOffset ReadAt, MeterSnapshot Meters, ReadingSource Source, string Reference, bool IsReset, string? Note);

/// <summary>
/// Tüm veri kaynaklarının ortak kayıt yolu (KK2–KK5). Aynı kaynak+referans ikinci kez gelirse başarı döner,
/// yeni okuma oluşmaz.
/// </summary>
public sealed class MeterReadingRecorder(ISlotMachineRepository machines, IFloorEvents events, IFloorUnitOfWork unitOfWork)
{
    /// <param name="auditKind">Verilirse başarılı kayıtla aynı commit'te denetim olayı yazılır.</param>
    public async Task<Result> Record(
        TenantId tenantId, MeterReadingInput input, AuditContext audit, CancellationToken ct, FloorChangeKind? auditKind = null)
    {
        ArgumentNullException.ThrowIfNull(input);
        var reference = input.Reference?.Trim() ?? string.Empty;
        if (reference.Length == 0)
        {
            return SlotErrors.ReferenceRequired;
        }

        if (await machines.ReadingExists(tenantId, input.Source, reference, ct))
        {
            return Result.Success();
        }

        var machine = await machines.GetByAsset(tenantId, input.AssetNumber, ct);
        if (machine is null)
        {
            return SlotErrors.MachineNotFound;
        }

        var recorded = machine.Record(new MeterReading(Guid.NewGuid(), input.ReadAt, input.Meters, input.Source, reference, input.IsReset, input.Note, audit));
        if (recorded.IsFailure)
        {
            return recorded;
        }

        machines.AddReading(recorded.Value);
        if (auditKind is { } kind)
        {
            events.Publish(tenantId, new FloorChange(Guid.NewGuid(), kind, $"{machine.AssetNumber} {input.ReadAt:u}", audit, input.ReadAt));
        }

        await unitOfWork.Commit(ct);
        return Result.Success();
    }
}
