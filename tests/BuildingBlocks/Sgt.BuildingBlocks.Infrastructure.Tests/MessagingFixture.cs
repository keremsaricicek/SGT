using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Sgt.BuildingBlocks.Infrastructure.Messaging;

namespace Sgt.BuildingBlocks.Infrastructure.Tests;

public sealed record SampleHappenedV1(Guid EventId, Guid CorrelationId, DateTimeOffset OccurredAt, TenantId TenantId, AuditContext Audit, string Summary)
    : IAuditableEvent;

public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("msgtest");
        modelBuilder.AddOutboxAndInbox();
    }
}

public sealed class ManualClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
}

/// <summary>Çağrıları kaydeden tüketici; Fail true ise hata fırlatır.</summary>
public sealed class RecordingHandler : IIntegrationEventHandler<SampleHappenedV1>, IIntegrationEventHandler<IAuditableEvent>
{
    public List<Guid> Received { get; } = [];

    public List<Guid> ReceivedAsAuditable { get; } = [];

    public bool Fail { get; set; }

    public Task Handle(SampleHappenedV1 integrationEvent, CancellationToken ct)
    {
        if (Fail)
        {
            throw new InvalidOperationException("tüketici hatası");
        }

        Received.Add(integrationEvent.EventId);
        return Task.CompletedTask;
    }

    Task IIntegrationEventHandler<IAuditableEvent>.Handle(IAuditableEvent integrationEvent, CancellationToken ct)
    {
        ReceivedAsAuditable.Add(integrationEvent.EventId);
        return Task.CompletedTask;
    }
}

public sealed class MessagingFixture : IAsyncLifetime
{
    private readonly string _connectionString = new NpgsqlConnectionStringBuilder(
        Environment.GetEnvironmentVariable("SGT_TEST_DB") ?? "Host=localhost;Username=sgt;Password=sgt_dev_only")
    {
        Database = $"sgt_msg_test_{Guid.NewGuid():N}",
    }.ConnectionString;

    public ManualClock Clock { get; } = new();

    public RecordingHandler Handler { get; } = new();

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IClock>(Clock);
        services.AddDbContext<TestDbContext>(o => o.UseNpgsql(_connectionString));
        services.AddIntegrationEvents().AddIntegrationEventType<SampleHappenedV1>();
        services.AddSingleton<OutboxProcessor<TestDbContext>>();
        services.AddScoped<IOutboxStore>(sp => new OutboxStore<TestDbContext>("Test", sp.GetRequiredService<TestDbContext>(), Clock));
        services.AddSingleton<IIntegrationEventHandler<SampleHappenedV1>>(Handler);
        services.AddSingleton<IIntegrationEventHandler<IAuditableEvent>>(Handler);
        Services = services.BuildServiceProvider();

        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TestDbContext>().Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await using (var scope = Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<TestDbContext>().Database.EnsureDeletedAsync();
        }

        await Services.DisposeAsync();
    }
}
