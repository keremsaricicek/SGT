using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finance");

            migrationBuilder.CreateTable(
                name: "cashier_shifts",
                schema: "finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cashier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    opened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closing_note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cashier_shifts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "finance",
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
                schema: "finance",
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
                name: "cage_transactions",
                schema: "finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cage_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_cage_transactions_cashier_shifts_shift_id",
                        column: x => x.shift_id,
                        principalSchema: "finance",
                        principalTable: "cashier_shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shift_closing_counts",
                schema: "finance",
                columns: table => new
                {
                    shift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instrument = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    expected = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    counted = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shift_closing_counts", x => new { x.shift_id, x.instrument, x.currency });
                    table.ForeignKey(
                        name: "fk_shift_closing_counts_cashier_shifts_shift_id",
                        column: x => x.shift_id,
                        principalSchema: "finance",
                        principalTable: "cashier_shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shift_opening_amounts",
                schema: "finance",
                columns: table => new
                {
                    shift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    instrument = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shift_opening_amounts", x => new { x.shift_id, x.instrument, x.currency });
                    table.ForeignKey(
                        name: "fk_shift_opening_amounts_cashier_shifts_shift_id",
                        column: x => x.shift_id,
                        principalSchema: "finance",
                        principalTable: "cashier_shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ledger_entries",
                schema: "finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    instrument = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ledger_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_ledger_entries_cage_transactions_transaction_id",
                        column: x => x.transaction_id,
                        principalSchema: "finance",
                        principalTable: "cage_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cage_transactions_shift_id",
                schema: "finance",
                table: "cage_transactions",
                column: "shift_id");

            migrationBuilder.CreateIndex(
                name: "ix_cage_transactions_tenant_id_occurred_at",
                schema: "finance",
                table: "cage_transactions",
                columns: new[] { "tenant_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_cage_transactions_tenant_id_player_id",
                schema: "finance",
                table: "cage_transactions",
                columns: new[] { "tenant_id", "player_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cashier_shifts_tenant_id_cashier_id",
                schema: "finance",
                table: "cashier_shifts",
                columns: new[] { "tenant_id", "cashier_id" },
                unique: true,
                filter: "closed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_cashier_shifts_tenant_id_terminal",
                schema: "finance",
                table: "cashier_shifts",
                columns: new[] { "tenant_id", "terminal" },
                unique: true,
                filter: "closed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_transaction_id",
                schema: "finance",
                table: "ledger_entries",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_next_attempt_at",
                schema: "finance",
                table: "outbox_messages",
                column: "next_attempt_at",
                filter: "processed_at IS NULL AND dead_lettered = false");

            // Değer kayıtları değiştirilemez ve silinemez; defter işlem/para birimi bazında dengelidir (Spec 006 KK7).
            migrationBuilder.Sql(@"
CREATE FUNCTION finance.reject_value_record_mutation() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION '% append-only: % yasak', TG_TABLE_NAME, TG_OP;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER cage_transactions_append_only BEFORE UPDATE OR DELETE ON finance.cage_transactions
FOR EACH ROW EXECUTE FUNCTION finance.reject_value_record_mutation();
CREATE TRIGGER ledger_entries_append_only BEFORE UPDATE OR DELETE ON finance.ledger_entries
FOR EACH ROW EXECUTE FUNCTION finance.reject_value_record_mutation();
CREATE TRIGGER shift_opening_amounts_append_only BEFORE UPDATE OR DELETE ON finance.shift_opening_amounts
FOR EACH ROW EXECUTE FUNCTION finance.reject_value_record_mutation();
CREATE TRIGGER shift_closing_counts_append_only BEFORE UPDATE OR DELETE ON finance.shift_closing_counts
FOR EACH ROW EXECUTE FUNCTION finance.reject_value_record_mutation();

CREATE FUNCTION finance.check_ledger_balanced() RETURNS trigger AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM finance.ledger_entries
        WHERE transaction_id = NEW.transaction_id
        GROUP BY currency HAVING SUM(amount) <> 0) THEN
        RAISE EXCEPTION 'Dengesiz defter kaydı: işlem %', NEW.transaction_id;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE CONSTRAINT TRIGGER ledger_entries_balanced AFTER INSERT ON finance.ledger_entries
DEFERRABLE INITIALLY DEFERRED
FOR EACH ROW EXECUTE FUNCTION finance.check_ledger_balanced();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TRIGGER IF EXISTS ledger_entries_balanced ON finance.ledger_entries;
DROP TRIGGER IF EXISTS cage_transactions_append_only ON finance.cage_transactions;
DROP TRIGGER IF EXISTS ledger_entries_append_only ON finance.ledger_entries;
DROP TRIGGER IF EXISTS shift_opening_amounts_append_only ON finance.shift_opening_amounts;
DROP TRIGGER IF EXISTS shift_closing_counts_append_only ON finance.shift_closing_counts;
DROP FUNCTION IF EXISTS finance.check_ledger_balanced();
DROP FUNCTION IF EXISTS finance.reject_value_record_mutation();");

            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "ledger_entries",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "shift_closing_counts",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "shift_opening_amounts",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "cage_transactions",
                schema: "finance");

            migrationBuilder.DropTable(
                name: "cashier_shifts",
                schema: "finance");
        }
    }
}
