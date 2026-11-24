using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.GamingFloor.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlotMachines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "meter_readings",
                schema: "gaming_floor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    machine_id = table.Column<Guid>(type: "uuid", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    coin_in = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    coin_out = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    jackpot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    games_played = table.Column<long>(type: "bigint", nullable: false),
                    delta_coin_in = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    delta_coin_out = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    delta_jackpot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    delta_games_played = table.Column<long>(type: "bigint", nullable: false),
                    source = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    source_reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_reset = table.Column<bool>(type: "boolean", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_meter_readings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "slot_machines",
                schema: "gaming_floor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    area = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    denomination = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    last_read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_coin_in = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    last_coin_out = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    last_jackpot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    last_games_played = table.Column<long>(type: "bigint", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_slot_machines", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_machine_id_read_at",
                schema: "gaming_floor",
                table: "meter_readings",
                columns: new[] { "machine_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_tenant_id_read_at",
                schema: "gaming_floor",
                table: "meter_readings",
                columns: new[] { "tenant_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "ix_meter_readings_tenant_id_source_source_reference",
                schema: "gaming_floor",
                table: "meter_readings",
                columns: new[] { "tenant_id", "source", "source_reference" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_slot_machines_tenant_id_asset_number",
                schema: "gaming_floor",
                table: "slot_machines",
                columns: new[] { "tenant_id", "asset_number" },
                unique: true);

            // Sayaç okumaları silinemez ve değiştirilemez (Spec 009 KK4).
            migrationBuilder.Sql(@"
CREATE TRIGGER meter_readings_append_only BEFORE UPDATE OR DELETE ON gaming_floor.meter_readings
FOR EACH ROW EXECUTE FUNCTION gaming_floor.reject_correction_mutation();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS meter_readings_append_only ON gaming_floor.meter_readings;");

            migrationBuilder.DropTable(
                name: "meter_readings",
                schema: "gaming_floor");

            migrationBuilder.DropTable(
                name: "slot_machines",
                schema: "gaming_floor");
        }
    }
}
