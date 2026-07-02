using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.IdentityMigrations
{
    /// <inheritdoc />
    public partial class AddOrganizationAndAddToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                    name: "Organization",
                    columns: table => new
                    {
                        Id = table.Column<string>(type: "varchar(255)", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4"),
                        Name = table.Column<string>(type: "longtext", nullable: false)
                            .Annotation("MySql:CharSet", "utf8mb4")
                    },
                    constraints: table => { table.PrimaryKey("PK_Organization", x => x.Id); })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Organization",
                columns: new[] { "Id", "Name" },
                values: new object[] { "1", "TestOrganization" });

            migrationBuilder.AddColumn<string>(
                    name: "OrganizationId",
                    table: "AspNetUsers",
                    type: "VARCHAR(255)",
                    nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");


            migrationBuilder.Sql("UPDATE planer_identity_db.AspNetUsers SET OrganizationId = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organization_OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Organization");
        }
    }
}