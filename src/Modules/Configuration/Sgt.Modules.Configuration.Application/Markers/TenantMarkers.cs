using Sgt.BuildingBlocks;
using Sgt.Modules.Configuration.Application.Ports;
using Sgt.Modules.Configuration.Contracts;
using Sgt.Modules.Configuration.Domain;

namespace Sgt.Modules.Configuration.Application.Markers;

public sealed record SetTenantMarkerCommand(TenantId TenantId, string Code, AuditContext Audit);

/// <summary>İşaret koyar; aynı işaret ikinci kez konamaz. Denetim olayı aynı transaction'da outbox'a yazılır.</summary>
public sealed class SetTenantMarkerHandler(
    ITenantMarkerStore markers,
    IConfigurationOutbox outbox,
    IConfigurationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SetTenantMarkerCommand>
{
    public async Task<Result> Handle(SetTenantMarkerCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        var marker = TenantMarker.Set(command.TenantId, command.Code, command.Audit, clock.UtcNow);
        if (marker.IsFailure)
        {
            return marker;
        }

        var m = marker.Value;
        if (await markers.Exists(m.TenantId, m.Code, ct))
        {
            return ConfigurationErrors.MarkerAlreadySet;
        }

        markers.Add(m);
        outbox.Add(new TenantMarkerSetV1(Guid.NewGuid(), m.Audit.CorrelationId, m.SetAt, m.TenantId, m.Audit, m.Code));
        try
        {
            await unitOfWork.Commit(ct);
        }
        catch (ConcurrencyConflictException)
        {
            // Bu commit'teki tek benzersiz kayıt işarettir: eşzamanlı başka bir istek aynı işareti koymuştur.
            return ConfigurationErrors.MarkerAlreadySet;
        }

        return Result.Success();
    }
}

public sealed record IsTenantMarkerSetQuery(TenantId TenantId, string Code);

public sealed class IsTenantMarkerSetHandler(ITenantMarkerStore markers) : IQueryHandler<IsTenantMarkerSetQuery, bool>
{
    public async Task<bool> Handle(IsTenantMarkerSetQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        var code = TenantMarker.Normalize(query.Code);
        return code is not null && await markers.Exists(query.TenantId, code, ct);
    }
}
