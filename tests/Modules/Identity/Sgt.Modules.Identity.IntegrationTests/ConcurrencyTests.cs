using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.IntegrationTests;

public sealed class ConcurrencyTests(IdentityDatabaseFixture db) : IClassFixture<IdentityDatabaseFixture>
{
    private static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));

    [Fact]
    public async Task Two_admins_editing_the_same_role_at_once_cannot_silently_overwrite_each_other()
    {
        var actor = AdminActor.From(Tenant, new AuditContext(UserId.New(), "PC", ReasonCode.Of("TEST"), Guid.NewGuid()), true).Value;
        var roleId = RoleId.New();
        await using (var setup = db.Services.CreateAsyncScope())
        {
            setup.ServiceProvider.GetRequiredService<IRoleRepository>().Add(Role.Create(roleId, "Kasiyer", actor, DateTimeOffset.UtcNow).Value);
            await setup.ServiceProvider.GetRequiredService<Sgt.Modules.Identity.Application.Ports.IIdentityUnitOfWork>().Commit(default);
        }

        await using var first = db.Services.CreateAsyncScope();
        await using var second = db.Services.CreateAsyncScope();
        var roleInFirst = (await first.ServiceProvider.GetRequiredService<IRoleRepository>().Get(Tenant, roleId, default))!;
        var roleInSecond = (await second.ServiceProvider.GetRequiredService<IRoleRepository>().Get(Tenant, roleId, default))!;

        roleInFirst.Grant(PermissionCode.Parse("cage.cash:view").Value, actor, DateTimeOffset.UtcNow);
        await first.ServiceProvider.GetRequiredService<Sgt.Modules.Identity.Application.Ports.IIdentityUnitOfWork>().Commit(default);

        roleInSecond.Grant(PermissionCode.Parse("slot.floor:view").Value, actor, DateTimeOffset.UtcNow);
        await Assert.ThrowsAsync<ConcurrencyConflictException>(
            () => second.ServiceProvider.GetRequiredService<Sgt.Modules.Identity.Application.Ports.IIdentityUnitOfWork>().Commit(default));
    }
}
