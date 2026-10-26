using Microsoft.AspNetCore.Authentication.JwtBearer;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.Audit.Api;
using Sgt.Modules.Audit.Infrastructure;
using Sgt.Modules.Identity.Api;
using Sgt.Modules.Identity.Domain;
using Sgt.Modules.Identity.Infrastructure;

// Tek host: modülleri kaydeder, kendi iş kodu yoktur (docs/kod-kurallari.md 3.2).
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ConcurrencyConflictHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IRequestContext, HttpRequestContext>();

// Kimlik: Keycloak (OIDC) token'ı doğrulanır; yetki SGT yetki panelindedir (ADR-0003).
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority = builder.Configuration["Authentication:Authority"];
        o.Audience = builder.Configuration["Authentication:Audience"];
        o.MapInboundClaims = false;
        o.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(SgtClaims.Policy, p => p.RequireAuthenticatedUser().RequireClaim(SgtClaims.UserId).RequireClaim(SgtClaims.TenantId));

var connectionString = builder.Configuration.GetConnectionString("Sgt")
    ?? throw new InvalidOperationException("ConnectionStrings:Sgt tanımlı değil.");
builder.Services.AddIdentityModule(connectionString);
builder.Services.AddAuditModule(connectionString);

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", (IClock clock) => Results.Ok(new { Status = "ok", ServerTimeUtc = clock.UtcNow }));
app.MapIdentityEndpoints();
app.MapAuditEndpoints();
app.MapOutboxAdminEndpoints();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.MigrateIdentityDatabase(default);
    await scope.ServiceProvider.MigrateAuditDatabase(default);

    var seed = app.Configuration.GetSection("DevelopmentSeed");
    if (Guid.TryParse(seed["TenantId"], out var tenant) && Guid.TryParse(seed["SuperAdminUserId"], out var admin))
    {
        await IdentityDevelopmentSeed.Run(scope.ServiceProvider, new TenantId(tenant), new UserId(admin), [Product.SPlus], default);
    }
}

await app.RunAsync();

/// <summary>API testlerinin host'u başlatabilmesi için.</summary>
public partial class Program;
