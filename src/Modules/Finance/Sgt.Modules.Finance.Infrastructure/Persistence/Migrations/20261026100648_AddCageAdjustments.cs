using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.Finance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCageAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "reverses_id",
                schema: "finance",
                table: "cage_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cage_adjustment_requests",
                schema: "finance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    requested_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    decided_by = table.Column<Guid>(type: "uuid", nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    reversal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cage_adjustment_requests", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cage_transactions_reverses_id",
                schema: "finance",
                table: "cage_transactions",
                column: "reverses_id",
                unique: true,
                filter: "reverses_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_cage_adjustment_requests_tenant_id_shift_id",
                schema: "finance",
                table: "cage_adjustment_requests",
                columns: new[] { "tenant_id", "shift_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cage_adjustment_requests",
                schema: "finance");

            migrationBuilder.DropIndex(
                name: "ix_cage_transactions_reverses_id",
                schema: "finance",
                table: "cage_transactions");

            migrationBuilder.DropColumn(
                name: "reverses_id",
                schema: "finance",
                table: "cage_transactions");
        }
    }
}
