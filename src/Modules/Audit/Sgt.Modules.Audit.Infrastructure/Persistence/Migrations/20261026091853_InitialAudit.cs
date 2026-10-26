using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "audit_entries",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    summary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "audit",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    handler = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_messages", x => new { x.event_id, x.handler });
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    next_attempt_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    dead_lettered = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_tenant_id_actor_id_occurred_at",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "tenant_id", "actor_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_tenant_id_correlation_id",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "tenant_id", "correlation_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_entries_tenant_id_occurred_at",
                schema: "audit",
                table: "audit_entries",
                columns: new[] { "tenant_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_next_attempt_at",
                schema: "audit",
                table: "outbox_messages",
                column: "next_attempt_at",
                filter: "processed_at IS NULL AND dead_lettered = false");

            // Denetim kayıtları silinemez ve değiştirilemez (Spec 002 KK7).
            migrationBuilder.Sql(@"
CREATE FUNCTION audit.reject_audit_entry_mutation() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'audit.audit_entries append-only: % yasak', TG_OP;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER audit_entries_append_only
BEFORE UPDATE OR DELETE ON audit.audit_entries
FOR EACH ROW EXECUTE FUNCTION audit.reject_audit_entry_mutation();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS audit_entries_append_only ON audit.audit_entries;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS audit.reject_audit_entry_mutation();");

            migrationBuilder.DropTable(
                name: "audit_entries",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "audit");
        }
    }
}
