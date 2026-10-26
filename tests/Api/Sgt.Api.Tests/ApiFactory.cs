using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Sgt.BuildingBlocks;
using Sgt.Modules.Audit.Infrastructure;
using Sgt.Modules.Identity.Domain;
using Sgt.Modules.Identity.Infrastructure;
using Sgt.Modules.Identity.Infrastructure.Persistence;

namespace Sgt.Api.Tests;

/// <summary>
/// Gerçek host + geçici PostgreSQL veritabanı. Keycloak yerine test kimlik şeması:
/// istek "X-Test-User" ve "X-Test-Tenant" başlıklarıyla kimlik taşır.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly TenantId Tenant = new(Guid.Parse("00000000-0000-0000-0000-0000000000a1"));
    public static readonly UserId SuperAdmin = new(Guid.Parse("00000000-0000-0000-0000-000000000001"));

    private const string LocalServer = "Host=localhost;Username=sgt;Password=sgt_dev_only";

    private readonly string _connectionString = new NpgsqlConnectionStringBuilder(
        Environment.GetEnvironmentVariable("SGT_TEST_DB") ?? LocalServer)
    {
        Database = $"sgt_api_test_{Guid.NewGuid():N}",
    }.ConnectionString;

    public string ConnectionString => _connectionString;

    public HttpClient ClientFor(UserId user, TenantId? tenant = null)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, user.Value.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantHeader, (tenant ?? Tenant).Value.ToString());
        client.DefaultRequestHeaders.Add("X-Terminal", "TEST-PC");
        return client;
    }

    public async Task AddUser(UserId user, bool isSuperAdmin = false)
    {
        await using var scope = Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<Sgt.Modules.Identity.Application.Ports.IUserAccessRepository>();
        users.Add(UserAccess.Create(user, Tenant, isSuperAdmin));
        await scope.ServiceProvider.GetRequiredService<Sgt.Modules.Identity.Application.Ports.IIdentityUnitOfWork>().Commit(default);
    }

    public async Task InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.MigrateIdentityDatabase(default);
        await scope.ServiceProvider.MigrateAuditDatabase(default);
        await IdentityDevelopmentSeed.Run(scope.ServiceProvider, Tenant, SuperAdmin, [Product.SPlus], default);
    }

    public new async Task DisposeAsync()
    {
        await using (var scope = Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Sgt", _connectionString);
        builder.ConfigureServices(services => services
            .AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { }));
        builder.ConfigureServices(services => services.PostConfigure<AuthenticationOptions>(o =>
        {
            o.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
            o.DefaultChallengeScheme = TestAuthHandler.SchemeName;
        }));
    }
}

internal sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserHeader = "X-Test-User";
    public const string TenantHeader = "X-Test-Tenant";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var user) || !Request.Headers.TryGetValue(TenantHeader, out var tenant))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity([new Claim("sub", user!), new Claim("tenant_id", tenant!)], SchemeName);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
