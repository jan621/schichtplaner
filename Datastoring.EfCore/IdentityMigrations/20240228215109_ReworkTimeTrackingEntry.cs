using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class ReworkTimeTrackingEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "TimeTrackingEntries");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Start",
                table: "TimeTrackingEntries",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "End",
                table: "TimeTrackingEntries",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "Start",
                table: "TimeTrackingEntries",
                type: "time(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "End",
                table: "TimeTrackingEntries",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "TimeTrackingEntries",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
