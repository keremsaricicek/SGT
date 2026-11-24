using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Menu;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Application.Roles.CreateRole;
using Sgt.Modules.Identity.Application.Roles.GrantRolePermission;
using Sgt.Modules.Identity.Application.Users.AssignRole;
using Sgt.Modules.Identity.Contracts;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.IntegrationTests;

public sealed class PersistenceTests(IdentityDatabaseFixture db) : IClassFixture<IdentityDatabaseFixture>
{
    private static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    private static readonly UserId Admin = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private static AuditContext Audit => new(Admin, "TEST-PC", ReasonCode.Of("ROLE_SETUP"), Guid.NewGuid());

    [Fact]
    public async Task Role_grant_and_assignment_survive_a_new_request_and_are_logged()
    {
        await AddUser(Admin, isSuperAdmin: true);
        var staff = UserId.New();
        await AddUser(staff, isSuperAdmin: false);

        var roleId = await Send<CreateRoleCommand, RoleId>(new CreateRoleCommand(Tenant, "Kasiyer", Audit));
        await Send(new GrantRolePermissionCommand(Tenant, roleId, "cage.adjustment:view", Audit));
        await Send(new AssignRoleCommand(Tenant, staff, roleId, Audit));

        await using var scope = db.Services.CreateAsyncScope();
        var role = await scope.ServiceProvider.GetRequiredService<IRoleRepository>().Get(Tenant, roleId, default);
        var user = await scope.ServiceProvider.GetRequiredService<IUserAccessRepository>().Get(Tenant, staff, default);

        Assert.True(role!.Permissions.Contains(PermissionCode.Parse("cage.adjustment:view").Value));
        Assert.Contains(roleId, user!.RoleIds);
        Assert.Equal(3, await Count($"SELECT count(*) FROM identity.permission_changes WHERE target_id IN ('{roleId.Value}', '{staff.Value}')"));
        Assert.Equal(3, await Count(
            "SELECT count(*) FROM identity.outbox_messages o JOIN identity.permission_changes c ON c.id = o.id " +
            $"WHERE o.type = 'Sgt.Modules.Identity.Contracts.PermissionChangedV1' AND c.target_id IN ('{roleId.Value}', '{staff.Value}')"));
    }

    [Fact]
    public async Task KK6_permission_changes_cannot_be_updated_or_deleted()
    {
        await AddUser(Admin, isSuperAdmin: true);
        await Send<CreateRoleCommand, RoleId>(new CreateRoleCommand(Tenant, "Pit boss", Audit));

        await Assert.ThrowsAsync<PostgresException>(() => Execute("UPDATE identity.permission_changes SET item = 'x'"));
        await Assert.ThrowsAsync<PostgresException>(() => Execute("DELETE FROM identity.permission_changes"));
    }

    [Fact]
    public async Task KK8_other_tenant_cannot_read_roles()
    {
        await AddUser(Admin, isSuperAdmin: true);
        var roleId = await Send<CreateRoleCommand, RoleId>(new CreateRoleCommand(Tenant, "Slot müdürü", Audit));

        await using var scope = db.Services.CreateAsyncScope();
        var other = new TenantId(Guid.NewGuid());

        Assert.Null(await scope.ServiceProvider.GetRequiredService<IRoleRepository>().Get(other, roleId, default));
    }

    private async Task AddUser(UserId userId, bool isSuperAdmin)
    {
        await using var scope = db.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserAccessRepository>();
        if (await users.Get(Tenant, userId, default) is null)
        {
            users.Add(UserAccess.Create(userId, Tenant, isSuperAdmin));
            await scope.ServiceProvider.GetRequiredService<Sgt.Modules.Identity.Application.Ports.IIdentityUnitOfWork>().Commit(default);
        }
    }

    private async Task Send<TCommand>(TCommand command)
    {
        await using var scope = db.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>().Handle(command, default);
        Assert.True(result.IsSuccess, result.Error.Code);
    }

    private async Task<TResult> Send<TCommand, TResult>(TCommand command)
    {
        await using var scope = db.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>().Handle(command, default);
        Assert.True(result.IsSuccess, result.Error.Code);
        return result.Value;
    }

    private async Task<long> Count(string sql)
    {
        await using var connection = new NpgsqlConnection(db.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private async Task Execute(string sql)
    {
        await using var connection = new NpgsqlConnection(db.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
