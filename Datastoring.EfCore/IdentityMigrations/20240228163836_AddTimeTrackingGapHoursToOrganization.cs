using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class AddTimeTrackingGapHoursToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "TimeTrackingGapHours",
                table: "Organization",
                type: "tinyint unsigned",
                nullable: false,
                defaultValue: (byte)8); //Default value should be 8 hours
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeTrackingGapHours",
                table: "Organization");
        }
    }
}
