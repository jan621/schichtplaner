using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class MakeTimeTrackingEndNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "End",
                table: "TimeTrackingEntries",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "End",
                table: "TimeTrackingEntries",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)",
                oldNullable: true);
        }
    }
}
