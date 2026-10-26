using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Sgt.BuildingBlocks.Web;

/// <summary>Operatör ekranı: tüm modüllerin dead-letter olayları ve yeniden işletme (Spec 002 KK5).</summary>
public sealed record ReprocessRequest(string ReasonCode);

public static partial class OutboxAdminEndpoints
{
    public static readonly Error NotFound =
        new("Integration.DeadLetterNotFound", "Bu kimlikte dead-letter olay yok.", ErrorKind.NotFound);

    public static IEndpointRouteBuilder MapOutboxAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1/integration").RequireAuthorization(SgtClaims.Policy).WithTags("Integration");

        api.MapGet("/dead-letters", async (int? take, IRequestContext request, IEnumerable<IOutboxStore> stores, CancellationToken ct) =>
        {
            var all = new List<DeadLetter>();
            foreach (var store in stores)
            {
                all.AddRange(await store.DeadLetters(request.TenantId, Math.Clamp(take ?? 100, 1, 500), ct));
            }

            return Results.Ok(all.OrderByDescending(d => d.OccurredAt));
        }).RequirePermission("admin.integrations:view");

        api.MapPost("/dead-letters/{module}/{eventId:guid}/reprocess", async (string module, Guid eventId, ReprocessRequest body, IRequestContext request, IEnumerable<IOutboxStore> stores, ILoggerFactory logs, CancellationToken ct) =>
        {
            var reason = ReasonCode.Create(body?.ReasonCode);
            if (reason.IsFailure)
            {
                return ResultHttpExtensions.Problem(reason.Error);
            }

            var audit = request.Audit(reason.Value);
            LogReprocess(logs.CreateLogger("Sgt.Integration"), module, eventId, audit.Actor.Value, audit.Terminal, audit.Reason.Value, audit.CorrelationId);
            var store = stores.FirstOrDefault(s => string.Equals(s.Module, module, StringComparison.OrdinalIgnoreCase));
            return store is not null && await store.Reprocess(request.TenantId, eventId, ct)
                ? Results.NoContent()
                : ResultHttpExtensions.Problem(NotFound);
        }).RequirePermission("admin.integrations:reprocess");

        return app;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Dead-letter yeniden işletildi: {Module}/{EventId} aktör {Actor} terminal {Terminal} gerekçe {Reason} correlation {CorrelationId}")]
    private static partial void LogReprocess(ILogger logger, string module, Guid eventId, Guid actor, string terminal, string reason, Guid correlationId);
}
