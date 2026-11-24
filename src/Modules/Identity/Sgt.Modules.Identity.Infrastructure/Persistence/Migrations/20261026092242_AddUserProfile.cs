using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sgt.Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "display_name",
                schema: "identity",
                table: "user_access",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "employee_code",
                schema: "identity",
                table: "user_access",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "identity",
                table: "user_access",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_access_tenant_id_employee_code",
                schema: "identity",
                table: "user_access",
                columns: new[] { "tenant_id", "employee_code" },
                unique: true,
                filter: "employee_code <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_access_tenant_id_employee_code",
                schema: "identity",
                table: "user_access");

            migrationBuilder.DropColumn(
                name: "display_name",
                schema: "identity",
                table: "user_access");

            migrationBuilder.DropColumn(
                name: "employee_code",
                schema: "identity",
                table: "user_access");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "identity",
                table: "user_access");
        }
    }
}
