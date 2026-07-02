using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class AddSmoobuApiKeyToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SmoobuApiKey",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmoobuApiKey",
                table: "AspNetUsers");
        }
    }
}
