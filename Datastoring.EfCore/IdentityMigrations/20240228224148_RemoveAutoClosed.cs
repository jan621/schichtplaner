using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class RemoveAutoClosed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoClosed",
                table: "TimeTrackingEntries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoClosed",
                table: "TimeTrackingEntries",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
