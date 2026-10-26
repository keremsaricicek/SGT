using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Sgt.BuildingBlocks;
using Sgt.BuildingBlocks.Infrastructure.Security;
using Sgt.BuildingBlocks.Web;
using Sgt.Modules.Audit.Api;
using Sgt.Modules.Audit.Infrastructure;
using Sgt.Modules.CasinoPlayer.Api;
using Sgt.Modules.CasinoPlayer.Infrastructure;
using Sgt.Modules.Configuration.Api;
using Sgt.Modules.Configuration.Infrastructure;
using Sgt.Modules.Identity.Api;
using Sgt.Modules.Identity.Domain;
using Sgt.Modules.Identity.Infrastructure;

// Tek host: modülleri kaydeder, kendi iş kodu yoktur (docs/kod-kurallari.md 3.2).
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
// Enum'lar API'de okunabilir metin olarak taşınır (ör. "Passport").
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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
builder.Services.AddConfigurationModule(connectionString);
builder.Services.AddCasinoPlayerModule(connectionString);

// Kişisel veri anahtarları secret manager'dan gelir (ADR-0005).
builder.Services.AddSingleton<IPersonalDataProtector>(_ => AesGcmPersonalDataProtector.FromBase64(
    builder.Configuration["Security:PersonalDataKey"], builder.Configuration["Security:BlindIndexKey"]));

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", (IClock clock) => Results.Ok(new { Status = "ok", ServerTimeUtc = clock.UtcNow }));
app.MapIdentityEndpoints();
app.MapAuditEndpoints();
app.MapConfigurationEndpoints();
app.MapCasinoPlayerEndpoints();
app.MapOutboxAdminEndpoints();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.MigrateIdentityDatabase(default);
    await scope.ServiceProvider.MigrateAuditDatabase(default);
    await scope.ServiceProvider.MigrateConfigurationDatabase(default);
    await scope.ServiceProvider.MigrateCasinoPlayerDatabase(default);

    var seed = app.Configuration.GetSection("DevelopmentSeed");
    if (Guid.TryParse(seed["TenantId"], out var tenant) && Guid.TryParse(seed["SuperAdminUserId"], out var admin))
    {
        await IdentityDevelopmentSeed.Run(scope.ServiceProvider, new TenantId(tenant), new UserId(admin), [Product.SPlus], default);
    }
}

await app.RunAsync();

/// <summary>API testlerinin host'u başlatabilmesi için.</summary>
public partial class Program;
