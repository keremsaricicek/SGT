using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.Modules.Identity.Application.Access;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Menu;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Application.Roles.CreateRole;
using Sgt.Modules.Identity.Application.Roles.GrantRolePermission;
using Sgt.Modules.Identity.Application.Roles.RevokeRolePermission;
using Sgt.Modules.Identity.Application.Users.AssignRole;
using Sgt.Modules.Identity.Application.Users.DenyUserPermission;
using Sgt.Modules.Identity.Application.Users.GrantUserPermission;
using Sgt.Modules.Identity.Application.Users.RevokeUserPermission;
using Sgt.Modules.Identity.Application.Users.UnassignRole;
using Sgt.Modules.Identity.Application.Users.UndenyUserPermission;
using Sgt.Modules.Identity.Contracts;
using Sgt.Modules.Identity.Domain;
using Sgt.Modules.Identity.Infrastructure.Persistence;

namespace Sgt.Modules.Identity.Infrastructure;

/// <summary>Identity modülünün tüm bağımlılık kayıtları. Host yalnızca bunu çağırır.</summary>
public static class IdentityInfrastructureModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(o => o.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", IdentityDbContext.Schema))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<TrackedAggregates>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserAccessRepository, UserAccessRepository>();
        services.AddScoped<IUnitOfWork, IdentityUnitOfWork>();

        services.AddScoped<AdminActorResolver>();
        services.AddScoped<EffectiveAccess>();
        services.AddScoped<IPermissionQuery, PermissionQuery>();

        services.AddScoped<ICommandHandler<CreateRoleCommand, RoleId>, CreateRoleHandler>();
        services.AddScoped<ICommandHandler<GrantRolePermissionCommand>, GrantRolePermissionHandler>();
        services.AddScoped<ICommandHandler<RevokeRolePermissionCommand>, RevokeRolePermissionHandler>();
        services.AddScoped<ICommandHandler<AssignRoleCommand>, AssignRoleHandler>();
        services.AddScoped<ICommandHandler<UnassignRoleCommand>, UnassignRoleHandler>();
        services.AddScoped<ICommandHandler<GrantUserPermissionCommand>, GrantUserPermissionHandler>();
        services.AddScoped<ICommandHandler<RevokeUserPermissionCommand>, RevokeUserPermissionHandler>();
        services.AddScoped<ICommandHandler<DenyUserPermissionCommand>, DenyUserPermissionHandler>();
        services.AddScoped<ICommandHandler<UndenyUserPermissionCommand>, UndenyUserPermissionHandler>();
        services.AddScoped<IQueryHandler<GetMyMenuQuery, IReadOnlyList<MenuItem>>, GetMyMenuHandler>();

        return services;
    }

    public static Task MigrateIdentityDatabase(this IServiceProvider services, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<IdentityDbContext>().Database.MigrateAsync(ct);
    }
}
