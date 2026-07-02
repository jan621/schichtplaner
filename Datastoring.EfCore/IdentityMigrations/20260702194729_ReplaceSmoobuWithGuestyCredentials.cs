using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class ReplaceSmoobuWithGuestyCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmoobuApiKey",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "GuestyClientSecret",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GuestyAccessToken",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GuestyClientId",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "GuestyTokenExpiresAt",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuestyAccessToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GuestyClientId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GuestyTokenExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GuestyClientSecret",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "SmoobuApiKey",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
