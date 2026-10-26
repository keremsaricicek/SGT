using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.Loyalty.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLoyalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "loyalty");

            migrationBuilder.CreateTable(
                name: "accounts",
                schema: "loyalty",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_count = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => new { x.tenant_id, x.player_id });
                });

            migrationBuilder.CreateTable(
                name: "adjustment_requests",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    decided_by = table.Column<Guid>(type: "uuid", nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adjustment_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "comp_requests",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points = table.Column<long>(type: "bigint", nullable: false),
                    text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    decided_by = table.Column<Guid>(type: "uuid", nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comp_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "earning_rules",
                schema: "loyalty",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    try_per_point = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_earning_rules", x => x.tenant_id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "loyalty",
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
                schema: "loyalty",
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

            migrationBuilder.CreateTable(
                name: "points_entries",
                schema: "loyalty",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    points = table.Column<long>(type: "bigint", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_points_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_points_entries_accounts_tenant_id_player_id",
                        columns: x => new { x.tenant_id, x.player_id },
                        principalSchema: "loyalty",
                        principalTable: "accounts",
                        principalColumns: new[] { "tenant_id", "player_id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_adjustment_requests_tenant_id_player_id",
                schema: "loyalty",
                table: "adjustment_requests",
                columns: new[] { "tenant_id", "player_id" });

            migrationBuilder.CreateIndex(
                name: "ix_comp_requests_tenant_id_player_id",
                schema: "loyalty",
                table: "comp_requests",
                columns: new[] { "tenant_id", "player_id" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_next_attempt_at",
                schema: "loyalty",
                table: "outbox_messages",
                column: "next_attempt_at",
                filter: "processed_at IS NULL AND dead_lettered = false");

            migrationBuilder.CreateIndex(
                name: "ix_points_entries_tenant_id_player_id_source_id",
                schema: "loyalty",
                table: "points_entries",
                columns: new[] { "tenant_id", "player_id", "source_id" });

            // Puan hareketleri silinemez ve değiştirilemez (Spec 008 KK3).
            migrationBuilder.Sql(@"
CREATE FUNCTION loyalty.reject_points_mutation() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'loyalty.points_entries append-only: % yasak', TG_OP;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER points_entries_append_only BEFORE UPDATE OR DELETE ON loyalty.points_entries
FOR EACH ROW EXECUTE FUNCTION loyalty.reject_points_mutation();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS points_entries_append_only ON loyalty.points_entries;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS loyalty.reject_points_mutation();");

            migrationBuilder.DropTable(
                name: "adjustment_requests",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "comp_requests",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "earning_rules",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "points_entries",
                schema: "loyalty");

            migrationBuilder.DropTable(
                name: "accounts",
                schema: "loyalty");
        }
    }
}
