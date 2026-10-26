using Microsoft.EntityFrameworkCore;

namespace Sgt.BuildingBlocks.Infrastructure.Messaging;

public static class MessagingModelBuilderExtensions
{
    /// <summary>Modülün şemasına outbox_messages ve inbox_messages tablolarını ekler.</summary>
    public static ModelBuilder AddOutboxAndInbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(e =>
        {
            e.ToTable("outbox_messages");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasColumnName("id");
            e.Property(m => m.Type).HasColumnName("type").HasMaxLength(300);
            e.Property(m => m.Payload).HasColumnName("payload").HasColumnType("jsonb");
            e.Property(m => m.OccurredAt).HasColumnName("occurred_at");
            e.Property(m => m.NextAttemptAt).HasColumnName("next_attempt_at");
            e.Property(m => m.ProcessedAt).HasColumnName("processed_at");
            e.Property(m => m.Attempts).HasColumnName("attempts");
            e.Property(m => m.LastError).HasColumnName("last_error").HasMaxLength(2000);
            e.Property(m => m.DeadLettered).HasColumnName("dead_lettered");
            e.HasIndex(m => m.NextAttemptAt).HasFilter("processed_at IS NULL AND dead_lettered = false");
        });

        modelBuilder.Entity<InboxMessage>(e =>
        {
            e.ToTable("inbox_messages");
            e.HasKey(m => new { m.EventId, m.Handler });
            e.Property(m => m.EventId).HasColumnName("event_id");
            e.Property(m => m.Handler).HasColumnName("handler").HasMaxLength(300);
            e.Property(m => m.ProcessedAt).HasColumnName("processed_at");
        });

        return modelBuilder;
    }
}
