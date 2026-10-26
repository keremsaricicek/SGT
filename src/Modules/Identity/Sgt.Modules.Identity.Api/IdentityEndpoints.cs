using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.Identity.Application.Menu;
using Sgt.Modules.Identity.Application.Panel;
using Sgt.Modules.Identity.Application.Roles.CreateDefaultRoles;
using Sgt.Modules.Identity.Application.Roles.CreateRole;
using Sgt.Modules.Identity.Application.Roles.GrantRolePermission;
using Sgt.Modules.Identity.Application.Roles.RevokeRolePermission;
using Sgt.Modules.Identity.Application.Users.AssignRole;
using Sgt.Modules.Identity.Application.Users.DeactivateUser;
using Sgt.Modules.Identity.Application.Users.ReactivateUser;
using Sgt.Modules.Identity.Application.Users.RegisterUser;
using Sgt.Modules.Identity.Application.Users.DenyUserPermission;
using Sgt.Modules.Identity.Application.Users.GrantUserPermission;
using Sgt.Modules.Identity.Application.Users.RevokeUserPermission;
using Sgt.Modules.Identity.Application.Users.UnassignRole;
using Sgt.Modules.Identity.Application.Users.UndenyUserPermission;
using Sgt.Modules.Identity.Domain;

namespace Sgt.Modules.Identity.Api;

public sealed record CreateRoleRequest(string Name, string ReasonCode);

public sealed record ReasonRequest(string ReasonCode);

public sealed record PermissionRequest(string Permission, string ReasonCode);

public sealed record RoleAssignmentRequest(Guid RoleId, string ReasonCode);

public sealed record RegisterUserRequest(Guid UserId, string DisplayName, string EmployeeCode, string ReasonCode);

/// <summary>
/// Yetki paneli uç noktaları. Yalnızca HTTP ↔ komut/sorgu çevirisi yapar; iş kuralı yoktur.
/// Değişiklikleri süper admin kontrolü domain'de korur (KK5); okuma uçları ekran yetkisi ister (KK10).
/// </summary>
public static class IdentityEndpoints
{
    private const string PanelView = "admin.roles:view";
    private const string HistoryView = "admin.permission_history:view";

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/identity").RequireAuthorization(SgtClaims.Policy).WithTags("Identity");

        api.MapGet("/me/menu", async (IRequestContext request, IQueryHandler<GetMyMenuQuery, IReadOnlyList<MenuItem>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new GetMyMenuQuery(request.TenantId, request.UserId), ct)));

        api.MapGet("/screens", async (IRequestContext request, IQueryHandler<GetScreensQuery, IReadOnlyList<ScreenView>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new GetScreensQuery(request.TenantId), ct)))
            .RequirePermission(PanelView);

        api.MapGet("/roles", async (IRequestContext request, IQueryHandler<ListRolesQuery, IReadOnlyList<RoleView>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new ListRolesQuery(request.TenantId), ct)))
            .RequirePermission(PanelView);

        api.MapGet("/users/{userId:guid}/access", async (Guid userId, IRequestContext request, IQueryHandler<GetUserAccessQuery, UserAccessView?> handler, CancellationToken ct) =>
            await handler.Handle(new GetUserAccessQuery(request.TenantId, new UserId(userId)), ct) is { } view
                ? Results.Ok(view)
                : ResultHttpExtensions.Problem(IdentityErrors.UserNotFound))
            .RequirePermission(PanelView);

        api.MapGet("/permission-changes", async (int? take, DateTimeOffset? before, IRequestContext request, IQueryHandler<GetPermissionChangesQuery, IReadOnlyList<PermissionChangeView>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new GetPermissionChangesQuery(request.TenantId, take ?? 50, before), ct)))
            .RequirePermission(HistoryView);

        api.MapPost("/roles", async (CreateRoleRequest body, IRequestContext request, ICommandHandler<CreateRoleCommand, RoleId> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new CreateRoleCommand(request.TenantId, body.Name, audit), ct))
                .ToHttp(id => Results.Created($"/api/identity/roles/{id.Value}", new { id = id.Value }))));

        api.MapPost("/roles/defaults", async (ReasonRequest body, IRequestContext request, ICommandHandler<CreateDefaultRolesCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new CreateDefaultRolesCommand(request.TenantId, audit), ct)).ToHttp()));

        api.MapPost("/roles/{roleId:guid}/permissions", async (Guid roleId, PermissionRequest body, IRequestContext request, ICommandHandler<GrantRolePermissionCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new GrantRolePermissionCommand(request.TenantId, new RoleId(roleId), body.Permission, audit), ct)).ToHttp()));

        api.MapDelete("/roles/{roleId:guid}/permissions/{permission}", async (Guid roleId, string permission, string? reasonCode, IRequestContext request, ICommandHandler<RevokeRolePermissionCommand> handler, CancellationToken ct) =>
            await WithReason(reasonCode, request, async audit =>
                (await handler.Handle(new RevokeRolePermissionCommand(request.TenantId, new RoleId(roleId), permission, audit), ct)).ToHttp()));

        api.MapPost("/users/{userId:guid}/roles", async (Guid userId, RoleAssignmentRequest body, IRequestContext request, ICommandHandler<AssignRoleCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new AssignRoleCommand(request.TenantId, new UserId(userId), new RoleId(body.RoleId), audit), ct)).ToHttp()));

        api.MapDelete("/users/{userId:guid}/roles/{roleId:guid}", async (Guid userId, Guid roleId, string? reasonCode, IRequestContext request, ICommandHandler<UnassignRoleCommand> handler, CancellationToken ct) =>
            await WithReason(reasonCode, request, async audit =>
                (await handler.Handle(new UnassignRoleCommand(request.TenantId, new UserId(userId), new RoleId(roleId), audit), ct)).ToHttp()));

        api.MapPost("/users/{userId:guid}/grants", async (Guid userId, PermissionRequest body, IRequestContext request, ICommandHandler<GrantUserPermissionCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new GrantUserPermissionCommand(request.TenantId, new UserId(userId), body.Permission, audit), ct)).ToHttp()));

        api.MapDelete("/users/{userId:guid}/grants/{permission}", async (Guid userId, string permission, string? reasonCode, IRequestContext request, ICommandHandler<RevokeUserPermissionCommand> handler, CancellationToken ct) =>
            await WithReason(reasonCode, request, async audit =>
                (await handler.Handle(new RevokeUserPermissionCommand(request.TenantId, new UserId(userId), permission, audit), ct)).ToHttp()));

        api.MapPost("/users/{userId:guid}/denies", async (Guid userId, PermissionRequest body, IRequestContext request, ICommandHandler<DenyUserPermissionCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new DenyUserPermissionCommand(request.TenantId, new UserId(userId), body.Permission, audit), ct)).ToHttp()));

        api.MapDelete("/users/{userId:guid}/denies/{permission}", async (Guid userId, string permission, string? reasonCode, IRequestContext request, ICommandHandler<UndenyUserPermissionCommand> handler, CancellationToken ct) =>
            await WithReason(reasonCode, request, async audit =>
                (await handler.Handle(new UndenyUserPermissionCommand(request.TenantId, new UserId(userId), permission, audit), ct)).ToHttp()));

        api.MapGet("/users", async (IRequestContext request, IQueryHandler<ListUsersQuery, IReadOnlyList<UserSummary>> handler, CancellationToken ct) =>
            Results.Ok(await handler.Handle(new ListUsersQuery(request.TenantId), ct)))
            .RequirePermission("admin.users:view");

        api.MapPost("/users", async (RegisterUserRequest body, IRequestContext request, ICommandHandler<RegisterUserCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new RegisterUserCommand(request.TenantId, new UserId(body.UserId), body.DisplayName, body.EmployeeCode, audit), ct))
                .ToHttp()))
            .RequirePermission("admin.users:create");

        api.MapPost("/users/{userId:guid}/deactivate", async (Guid userId, ReasonRequest body, IRequestContext request, ICommandHandler<DeactivateUserCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new DeactivateUserCommand(request.TenantId, new UserId(userId), audit), ct)).ToHttp()))
            .RequirePermission("admin.users:deactivate");

        api.MapPost("/users/{userId:guid}/reactivate", async (Guid userId, ReasonRequest body, IRequestContext request, ICommandHandler<ReactivateUserCommand> handler, CancellationToken ct) =>
            await WithReason(body.ReasonCode, request, async audit =>
                (await handler.Handle(new ReactivateUserCommand(request.TenantId, new UserId(userId), audit), ct)).ToHttp()))
            .RequirePermission("admin.users:deactivate");

        return app;
    }

    // Her değişiklik isteği gerekçe kodu taşır (KK12).
    private static async Task<IResult> WithReason(string? reasonCode, IRequestContext request, Func<AuditContext, Task<IResult>> run)
    {
        var reason = ReasonCode.Create(reasonCode);
        return reason.IsSuccess ? await run(request.Audit(reason.Value)) : ResultHttpExtensions.Problem(reason.Error);
    }
}
