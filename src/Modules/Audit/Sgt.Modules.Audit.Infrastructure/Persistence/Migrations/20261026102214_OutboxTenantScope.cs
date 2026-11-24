using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.Audit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OutboxTenantScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                schema: "audit",
                table: "outbox_messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_tenant_id_dead_lettered",
                schema: "audit",
                table: "outbox_messages",
                columns: new[] { "tenant_id", "dead_lettered" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_tenant_id_dead_lettered",
                schema: "audit",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "audit",
                table: "outbox_messages");
        }
    }
}
