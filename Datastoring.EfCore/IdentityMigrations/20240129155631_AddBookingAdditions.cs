using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class AddBookingAdditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookingAdditionId",
                table: "AspNetUsers",
                type: "VARCHAR(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BookingAdditions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookingId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Organization = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingAdditions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BookingAdditionId",
                table: "AspNetUsers",
                column: "BookingAdditionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_BookingAdditions_BookingAdditionId",
                table: "AspNetUsers",
                column: "BookingAdditionId",
                principalTable: "BookingAdditions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_BookingAdditions_BookingAdditionId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BookingAdditions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BookingAdditionId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BookingAdditionId",
                table: "AspNetUsers");
        }
    }
}
