using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Sgt.BuildingBlocks.Web;

public static class PermissionEndpointFilter
{
    public static readonly Error Forbidden =
        new("Common.PermissionRequired", "Bu işlem için yetkiniz yok.", ErrorKind.Forbidden);

    /// <summary>Uç noktayı belirtilen ekran yetkisine bağlar; kontrol sunucuda yapılır (Spec 001 KK10).</summary>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(async (context, next) =>
        {
            var services = context.HttpContext.RequestServices;
            var request = services.GetRequiredService<IRequestContext>();
            var checker = services.GetRequiredService<IPermissionChecker>();

            var allowed = await checker.HasPermission(
                request.TenantId, request.UserId, permission, context.HttpContext.RequestAborted);
            return allowed ? await next(context) : ResultHttpExtensions.Problem(Forbidden);
        });
}
