using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.GamingFloor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialGamingFloor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gaming_floor");

            migrationBuilder.CreateTable(
                name: "gaming_tables",
                schema: "gaming_floor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    game_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    min_bet = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    max_bet = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    house_edge_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    hands_per_hour = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gaming_tables", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "gaming_floor",
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
                schema: "gaming_floor",
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
                name: "ratings",
                schema: "gaming_floor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    house_edge_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    hands_per_hour = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    started_by = table.Column<Guid>(type: "uuid", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    average_bet = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    player_result = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    theoretical_win = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ratings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "table_sessions",
                schema: "gaming_floor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    opened_by = table.Column<Guid>(type: "uuid", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_table_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_table_sessions_gaming_tables_table_id",
                        column: x => x.table_id,
                        principalSchema: "gaming_floor",
                        principalTable: "gaming_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rating_corrections",
                schema: "gaming_floor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    rating_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_average_bet = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    previous_player_result = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    average_bet = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    player_result = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rating_corrections", x => x.id);
                    table.ForeignKey(
                        name: "fk_rating_corrections_ratings_rating_id",
                        column: x => x.rating_id,
                        principalSchema: "gaming_floor",
                        principalTable: "ratings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_gaming_tables_tenant_id_code",
                schema: "gaming_floor",
                table: "gaming_tables",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_next_attempt_at",
                schema: "gaming_floor",
                table: "outbox_messages",
                column: "next_attempt_at",
                filter: "processed_at IS NULL AND dead_lettered = false");

            migrationBuilder.CreateIndex(
                name: "ix_rating_corrections_rating_id",
                schema: "gaming_floor",
                table: "rating_corrections",
                column: "rating_id");

            migrationBuilder.CreateIndex(
                name: "ix_ratings_tenant_id_player_id",
                schema: "gaming_floor",
                table: "ratings",
                columns: new[] { "tenant_id", "player_id" },
                unique: true,
                filter: "finished_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_ratings_tenant_id_table_id",
                schema: "gaming_floor",
                table: "ratings",
                columns: new[] { "tenant_id", "table_id" },
                filter: "finished_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_table_sessions_table_id",
                schema: "gaming_floor",
                table: "table_sessions",
                column: "table_id",
                unique: true,
                filter: "closed_at IS NULL");

            // Rating düzeltmeleri silinemez ve değiştirilemez (Spec 007 KK6).
            migrationBuilder.Sql(@"
CREATE FUNCTION gaming_floor.reject_correction_mutation() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'gaming_floor.rating_corrections append-only: % yasak', TG_OP;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER rating_corrections_append_only BEFORE UPDATE OR DELETE ON gaming_floor.rating_corrections
FOR EACH ROW EXECUTE FUNCTION gaming_floor.reject_correction_mutation();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS rating_corrections_append_only ON gaming_floor.rating_corrections;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS gaming_floor.reject_correction_mutation();");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "gaming_floor");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "gaming_floor");

            migrationBuilder.DropTable(
                name: "rating_corrections",
                schema: "gaming_floor");

            migrationBuilder.DropTable(
                name: "table_sessions",
                schema: "gaming_floor");

            migrationBuilder.DropTable(
                name: "ratings",
                schema: "gaming_floor");

            migrationBuilder.DropTable(
                name: "gaming_tables",
                schema: "gaming_floor");
        }
    }
}
