using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditableEntityToTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Tags",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Tags",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Organization",
                table: "Tags",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Tags",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Tags",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "Organization",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Tags");
        }
    }
}
