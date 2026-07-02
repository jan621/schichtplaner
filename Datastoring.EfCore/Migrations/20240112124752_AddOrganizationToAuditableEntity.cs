using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationToAuditableEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "MailTemplates",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Organization",
                table: "MailTemplates");
        }
    }
}
