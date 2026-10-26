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
using Sgt.Modules.Finance.Api;
using Sgt.Modules.Finance.Infrastructure;
using Sgt.Modules.GamingFloor.Api;
using Sgt.Modules.GamingFloor.Infrastructure;
using Sgt.Modules.Configuration.Infrastructure;
using Sgt.Modules.Identity.Api;
using Sgt.Modules.Loyalty.Api;
using Sgt.Modules.Loyalty.Infrastructure;
using Sgt.Modules.Reporting.Api;
using Sgt.Modules.Reporting.Infrastructure;
using Sgt.Modules.Identity.Domain;
using Sgt.Modules.Identity.Infrastructure;

// Tek host: modülleri kaydeder, kendi iş kodu yoktur (docs/kod-kurallari.md 3.2).
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService(o => o.ServiceName = "SGT API"); // Windows sunucuda servis olarak çalışır (ADR-0007)

builder.Services.AddProblemDetails();
// Web paneli farklı adresten çağırır; izinli adresler yapılandırmadan gelir (Cors:AllowedOrigins).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(allowedOrigins)
    .AllowAnyMethod()
    .WithHeaders("Authorization", "Content-Type", SgtHeaders.Terminal, SgtHeaders.CorrelationId)));
builder.Services.AddOpenApi("v1"); // sözleşme: docs/api/openapi-v1.json (ADR-0006)
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
builder.Services.AddFinanceModule(connectionString);
builder.Services.AddGamingFloorModule(connectionString);
builder.Services.AddLoyaltyModule(connectionString);
builder.Services.AddReportingModule();

// Kişisel veri anahtarları secret manager'dan gelir (ADR-0005).
builder.Services.AddSingleton<IPersonalDataProtector>(_ => AesGcmPersonalDataProtector.FromBase64(
    builder.Configuration["Security:PersonalDataKey"], builder.Configuration["Security:BlindIndexKey"]));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi(); // /openapi/v1.json
}

app.MapGet("/health", (IClock clock) => Results.Ok(new { Status = "ok", ServerTimeUtc = clock.UtcNow }));
app.MapIdentityEndpoints();
app.MapAuditEndpoints();
app.MapConfigurationEndpoints();
app.MapCasinoPlayerEndpoints();
app.MapFinanceEndpoints();
app.MapGamingFloorEndpoints();
app.MapLoyaltyEndpoints();
app.MapReportingEndpoints();
app.MapOutboxAdminEndpoints();

// "--migrate": yalnızca veritabanı şemasını günceller ve çıkar; üretimde dağıtım adımıdır (ADR-0007).
var migrateOnly = args.Contains("--migrate", StringComparer.Ordinal);
if (migrateOnly || app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.MigrateIdentityDatabase(default);
    await scope.ServiceProvider.MigrateAuditDatabase(default);
    await scope.ServiceProvider.MigrateConfigurationDatabase(default);
    await scope.ServiceProvider.MigrateCasinoPlayerDatabase(default);
    await scope.ServiceProvider.MigrateFinanceDatabase(default);
    await scope.ServiceProvider.MigrateGamingFloorDatabase(default);
    await scope.ServiceProvider.MigrateLoyaltyDatabase(default);

    if (migrateOnly)
    {
        return;
    }

    var seed = app.Configuration.GetSection("DevelopmentSeed");
    if (Guid.TryParse(seed["TenantId"], out var tenant) && Guid.TryParse(seed["SuperAdminUserId"], out var admin))
    {
        await IdentityDevelopmentSeed.Run(scope.ServiceProvider, new TenantId(tenant), new UserId(admin), [Product.SPlus], default);
    }
}

await app.RunAsync();

/// <summary>API testlerinin host'u başlatabilmesi için.</summary>
public partial class Program;
