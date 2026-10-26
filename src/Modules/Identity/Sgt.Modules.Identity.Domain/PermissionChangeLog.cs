using Sgt.BuildingBlocks;

namespace Sgt.Modules.Identity.Domain;

/// <summary>Bir aggregate'in henüz kaydedilmemiş değişiklik kayıtları.</summary>
internal sealed class PermissionChangeLog
{
    private readonly List<PermissionChange> _pending = [];
    private readonly TenantId _tenantId;
    private readonly PermissionChangeTarget _target;
    private readonly Guid _targetId;

    public PermissionChangeLog(TenantId tenantId, PermissionChangeTarget target, Guid targetId)
    {
        _tenantId = tenantId;
        _target = target;
        _targetId = targetId;
    }

    public IReadOnlyList<PermissionChange> Pending => _pending;

    public void Record(PermissionChangeKind kind, string item, AuditContext audit, DateTimeOffset at) =>
        _pending.Add(new PermissionChange(Guid.NewGuid(), _tenantId, _target, _targetId, kind, item, audit, at));

    public void Clear() => _pending.Clear();
}
