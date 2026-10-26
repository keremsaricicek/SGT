using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Messaging;
using Sgt.Modules.Identity.Application.Access;
using Sgt.Modules.Identity.Application.Administration;
using Sgt.Modules.Identity.Application.Menu;
using Sgt.Modules.Identity.Application.Panel;
using Sgt.Modules.Identity.Application.Ports;
using Sgt.Modules.Identity.Application.Roles.CreateDefaultRoles;
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
using Sgt.Modules.Identity.Infrastructure.Licensing;
using Sgt.Modules.Identity.Infrastructure.Persistence;
using Sgt.Modules.Identity.Infrastructure.Screens;

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

        services.AddOutbox<IdentityDbContext>("Identity");
        services.AddIntegrationEventType<PermissionChangedV1>();

        services.AddScoped<TrackedAggregates>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserAccessRepository, UserAccessRepository>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        services.AddScoped<IPermissionChangeReader, PermissionChangeReader>();
        services.AddScoped<ILicensedProducts, LicensedProducts>();
        services.AddSingleton<IScreenCatalog, ScreenCatalog>();

        services.AddScoped<AdminActorResolver>();
        services.AddScoped<EffectiveAccess>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();

        services.AddScoped<ICommandHandler<CreateRoleCommand, RoleId>, CreateRoleHandler>();
        services.AddScoped<ICommandHandler<GrantRolePermissionCommand>, GrantRolePermissionHandler>();
        services.AddScoped<ICommandHandler<RevokeRolePermissionCommand>, RevokeRolePermissionHandler>();
        services.AddScoped<ICommandHandler<AssignRoleCommand>, AssignRoleHandler>();
        services.AddScoped<ICommandHandler<UnassignRoleCommand>, UnassignRoleHandler>();
        services.AddScoped<ICommandHandler<GrantUserPermissionCommand>, GrantUserPermissionHandler>();
        services.AddScoped<ICommandHandler<RevokeUserPermissionCommand>, RevokeUserPermissionHandler>();
        services.AddScoped<ICommandHandler<DenyUserPermissionCommand>, DenyUserPermissionHandler>();
        services.AddScoped<ICommandHandler<UndenyUserPermissionCommand>, UndenyUserPermissionHandler>();
        services.AddScoped<ICommandHandler<CreateDefaultRolesCommand>, CreateDefaultRolesHandler>();
        services.AddScoped<IQueryHandler<GetMyMenuQuery, IReadOnlyList<MenuItem>>, GetMyMenuHandler>();
        services.AddScoped<IQueryHandler<GetScreensQuery, IReadOnlyList<ScreenView>>, GetScreensHandler>();
        services.AddScoped<IQueryHandler<ListRolesQuery, IReadOnlyList<RoleView>>, ListRolesHandler>();
        services.AddScoped<IQueryHandler<GetUserAccessQuery, UserAccessView?>, GetUserAccessHandler>();
        services.AddScoped<IQueryHandler<GetPermissionChangesQuery, IReadOnlyList<PermissionChangeView>>, GetPermissionChangesHandler>();

        return services;
    }

    public static Task MigrateIdentityDatabase(this IServiceProvider services, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredService<IdentityDbContext>().Database.MigrateAsync(ct);
    }
}
