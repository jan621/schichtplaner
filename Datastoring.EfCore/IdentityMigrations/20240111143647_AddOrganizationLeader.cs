using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class AddOrganizationLeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers");
            
            migrationBuilder.AddColumn<string>(
                name: "LeaderId",
                table: "Organization",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "5f6f4ca9-0c0b-485f-8df6-3281ea80d580") //USerId from Tobias Brenner Colony Creative
                .Annotation("MySql:CharSet", "utf8mb4");
            
            migrationBuilder.AlterColumn<string>(
                name: "OrganizationId",
                table: "AspNetUsers",
                type: "VARCHAR(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_LeaderId",
                table: "Organization",
                column: "LeaderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Organization_AspNetUsers_LeaderId",
                table: "Organization",
                column: "LeaderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Organization_AspNetUsers_LeaderId",
                table: "Organization");

            migrationBuilder.DropIndex(
                name: "IX_Organization_LeaderId",
                table: "Organization");

            migrationBuilder.DropColumn(
                name: "LeaderId",
                table: "Organization");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "OrganizationId",
                keyValue: null,
                column: "OrganizationId",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "OrganizationId",
                table: "AspNetUsers",
                type: "VARCHAR(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
