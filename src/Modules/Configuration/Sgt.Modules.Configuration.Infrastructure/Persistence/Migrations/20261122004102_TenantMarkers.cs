using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.Configuration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenantMarkers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenant_markers",
                schema: "configuration",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    set_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    terminal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reason_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_markers", x => new { x.tenant_id, x.code });
                });

            // İşaret bir kez konur; değiştirilemez ve silinemez (Spec 012).
            migrationBuilder.Sql(@"
CREATE FUNCTION configuration.reject_tenant_marker_mutation() RETURNS trigger AS $$
BEGIN
    RAISE EXCEPTION 'configuration.tenant_markers append-only: % yasak', TG_OP;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER tenant_markers_append_only
BEFORE UPDATE OR DELETE ON configuration.tenant_markers
FOR EACH ROW EXECUTE FUNCTION configuration.reject_tenant_marker_mutation();");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tenant_markers_append_only ON configuration.tenant_markers;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS configuration.reject_tenant_marker_mutation();");

            migrationBuilder.DropTable(
                name: "tenant_markers",
                schema: "configuration");
        }
    }
}
