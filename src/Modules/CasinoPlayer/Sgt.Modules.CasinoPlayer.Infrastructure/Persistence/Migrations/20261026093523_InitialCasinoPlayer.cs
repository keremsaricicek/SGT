using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.CasinoPlayer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCasinoPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "casino_player");

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "casino_player",
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
                schema: "casino_player",
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
                name: "player_sequences",
                schema: "casino_player",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_sequences", x => x.tenant_id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                schema: "casino_player",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    nationality = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    document_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    document_cipher = table.Column<string>(type: "text", nullable: false),
                    document_index = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    phone_cipher = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_players", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "player_cards",
                schema: "casino_player",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    issued_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cancelled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_cards", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_cards_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "casino_player",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_restrictions",
                schema: "casino_player",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    added_by = table.Column<Guid>(type: "uuid", nullable: false),
                    added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    removed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    removed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_restrictions", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_restrictions_players_player_id",
                        column: x => x.player_id,
                        principalSchema: "casino_player",
                        principalTable: "players",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_next_attempt_at",
                schema: "casino_player",
                table: "outbox_messages",
                column: "next_attempt_at",
                filter: "processed_at IS NULL AND dead_lettered = false");

            migrationBuilder.CreateIndex(
                name: "ix_player_cards_player_id",
                schema: "casino_player",
                table: "player_cards",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_cards_tenant_id_number",
                schema: "casino_player",
                table: "player_cards",
                columns: new[] { "tenant_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_player_restrictions_player_id",
                schema: "casino_player",
                table: "player_restrictions",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "ix_players_tenant_id_document_type_document_index",
                schema: "casino_player",
                table: "players",
                columns: new[] { "tenant_id", "document_type", "document_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_players_tenant_id_first_name",
                schema: "casino_player",
                table: "players",
                columns: new[] { "tenant_id", "first_name" });

            migrationBuilder.CreateIndex(
                name: "ix_players_tenant_id_last_name",
                schema: "casino_player",
                table: "players",
                columns: new[] { "tenant_id", "last_name" });

            migrationBuilder.CreateIndex(
                name: "ix_players_tenant_id_number",
                schema: "casino_player",
                table: "players",
                columns: new[] { "tenant_id", "number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "casino_player");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "casino_player");

            migrationBuilder.DropTable(
                name: "player_cards",
                schema: "casino_player");

            migrationBuilder.DropTable(
                name: "player_restrictions",
                schema: "casino_player");

            migrationBuilder.DropTable(
                name: "player_sequences",
                schema: "casino_player");

            migrationBuilder.DropTable(
                name: "players",
                schema: "casino_player");
        }
    }
}
