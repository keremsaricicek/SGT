using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Tests;

internal sealed class InMemoryRoles : IRoleRepository
{
    public List<Role> Items { get; } = [];

    public Task<Role?> Get(TenantId tenantId, RoleId roleId, CancellationToken ct) =>
        Task.FromResult(Items.SingleOrDefault(r => r.TenantId == tenantId && r.Id == roleId));

    public Task<IReadOnlyList<Role>> GetMany(TenantId tenantId, IEnumerable<RoleId> roleIds, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Role>>(Items.Where(r => r.TenantId == tenantId && roleIds.Contains(r.Id)).ToList());

    public Task<IReadOnlyList<Role>> List(TenantId tenantId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<Role>>(Items.Where(r => r.TenantId == tenantId).ToList());

    public void Add(Role role) => Items.Add(role);
}

internal sealed class InMemoryUsers : IUserAccessRepository
{
    public List<UserAccess> Items { get; } = [];

    public Task<UserAccess?> Get(TenantId tenantId, UserId userId, CancellationToken ct) =>
        Task.FromResult(Items.SingleOrDefault(u => u.TenantId == tenantId && u.UserId == userId));

    public void Add(UserAccess userAccess) => Items.Add(userAccess);

    public Task<IReadOnlyList<UserAccess>> List(TenantId tenantId, CancellationToken ct) =>
        Task.FromResult<IReadOnlyList<UserAccess>>(Items.Where(u => u.TenantId == tenantId).ToList());

    public Task<bool> EmployeeCodeExists(TenantId tenantId, string employeeCode, CancellationToken ct) =>
        Task.FromResult(Items.Any(u => u.TenantId == tenantId && string.Equals(u.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase)));

    public Task<int> CountActiveSuperAdmins(TenantId tenantId, CancellationToken ct) =>
        Task.FromResult(Items.Count(u => u.TenantId == tenantId && u.IsSuperAdmin && u.IsActive));
}

internal sealed class CountingUnitOfWork : IIdentityUnitOfWork
{
    public int Commits { get; private set; }

    public Task Commit(CancellationToken ct)
    {
        Commits++;
        return Task.CompletedTask;
    }
}

internal sealed class FixedClock : IClock
{
    public DateTimeOffset UtcNow => TestData.Now;
}

internal sealed class FixedScreens(params ScreenDefinition[] screens) : IScreenCatalog
{
    public IReadOnlyList<ScreenDefinition> All => screens;
}

internal sealed class FixedLicenses(params Product[] products) : ILicensedProducts
{
    public Task<IReadOnlySet<Product>> For(TenantId tenantId, CancellationToken ct) =>
        Task.FromResult<IReadOnlySet<Product>>(products.ToHashSet());
}
