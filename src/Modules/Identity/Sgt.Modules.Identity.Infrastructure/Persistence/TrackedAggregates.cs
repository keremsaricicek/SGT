using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Infrastructure.Persistence;

/// <summary>Bir istek içinde yüklenen veya eklenen aggregate'ler; Commit bunları kaydeder.</summary>
internal sealed class TrackedAggregates
{
    public HashSet<Role> Roles { get; } = [];

    public HashSet<UserAccess> Users { get; } = [];
}
