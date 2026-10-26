using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "permission_changes",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    item = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permission_changes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    permissions = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_access",
                schema: "identity",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_super_admin = table.Column<bool>(type: "boolean", nullable: false),
                    role_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    grants = table.Column<List<string>>(type: "text[]", nullable: false),
                    denied = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_access", x => new { x.tenant_id, x.user_id });
                });

            migrationBuilder.CreateIndex(
                name: "ix_permission_changes_tenant_id_occurred_at",
                schema: "identity",
                table: "permission_changes",
                columns: new[] { "tenant_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_roles_tenant_id",
                schema: "identity",
                table: "roles",
                column: "tenant_id");

            // Değişiklik kayıtları silinemez ve değiştirilemez (KK6).
            migrationBuilder.Sql(@"
CREATE FUNCTION identity.reject_permission_change_mutation() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'identity.permission_changes append-only: % yasak', TG_OP;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER permission_changes_append_only
BEFORE UPDATE OR DELETE ON identity.permission_changes
FOR EACH ROW EXECUTE FUNCTION identity.reject_permission_change_mutation();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS permission_changes_append_only ON identity.permission_changes;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS identity.reject_permission_change_mutation();");

            migrationBuilder.DropTable(
                name: "permission_changes",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "user_access",
                schema: "identity");
        }
    }
}
