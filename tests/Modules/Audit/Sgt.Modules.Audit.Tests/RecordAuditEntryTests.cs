using Sgt.BuildingBlocks;
using Sgt.Modules.Audit.Application.Ports;
using Sgt.Modules.Audit.Application.Recording;
using Sgt.Modules.Audit.Domain;

namespace Sgt.Modules.Audit.Tests;

public sealed record SomethingHappenedV1(Guid EventId, Guid CorrelationId, DateTimeOffset OccurredAt, TenantId TenantId, AuditContext Audit, string Summary)
    : IAuditableEvent;

public sealed class RecordAuditEntryTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeAuditLog _log = new();
    private readonly FakeInbox _inbox;
    private readonly CountingUnitOfWork _unitOfWork;

    public RecordAuditEntryTests()
    {
        _inbox = new FakeInbox();
        _unitOfWork = new CountingUnitOfWork(_inbox);
    }

    [Fact]
    public async Task KK6_auditable_event_becomes_an_audit_entry_with_full_context()
    {
        var e = NewEvent();

        await Handler().Handle(e, default);

        var entry = Assert.Single(_log.Entries);
        Assert.Equal(e.EventId, entry.Id);
        Assert.Equal(e.TenantId, entry.TenantId);
        Assert.Equal(typeof(SomethingHappenedV1).FullName, entry.EventType);
        Assert.Equal("kasa düzeltmesi", entry.Summary);
        Assert.Equal(e.Audit, entry.Audit);
        Assert.Equal(e.OccurredAt, entry.OccurredAt);
        Assert.Equal(Now, entry.RecordedAt);
        Assert.Equal(1, _unitOfWork.Commits);
    }

    [Fact]
    public async Task KK3_same_event_twice_records_once()
    {
        var e = NewEvent();

        await Handler().Handle(e, default);
        await Handler().Handle(e, default);

        Assert.Single(_log.Entries);
        Assert.Equal(1, _unitOfWork.Commits);
    }

    private static SomethingHappenedV1 NewEvent() => new(
        Guid.NewGuid(), Guid.NewGuid(), Now.AddMinutes(-1), new TenantId(Guid.NewGuid()),
        new AuditContext(UserId.New(), "KASA-1", ReasonCode.Of("CORRECTION"), Guid.NewGuid()), "kasa düzeltmesi");

    private RecordAuditEntryHandler Handler() => new(_log, _inbox, _unitOfWork, new FixedClock(Now));

    private sealed class FakeAuditLog : IAuditLog
    {
        public List<AuditEntry> Entries { get; } = [];

        public void Add(AuditEntry entry) => Entries.Add(entry);
    }

    private sealed class FakeInbox : IAuditInbox
    {
        private readonly HashSet<(Guid, string)> _pending = [];
        private readonly HashSet<(Guid, string)> _processed = [];

        public Task<bool> AlreadyProcessed(Guid eventId, string handler, CancellationToken ct) =>
            Task.FromResult(_processed.Contains((eventId, handler)));

        public void MarkProcessed(Guid eventId, string handler) => _pending.Add((eventId, handler));

        public void Commit()
        {
            _processed.UnionWith(_pending);
            _pending.Clear();
        }
    }

    // Gerçekte inbox kaydı unit of work commit'iyle yazılır; sahte de öyle davranır.
    private sealed class CountingUnitOfWork(FakeInbox inbox) : IAuditUnitOfWork
    {
        public int Commits { get; private set; }

        public Task Commit(CancellationToken ct)
        {
            Commits++;
            inbox.Commit();
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
