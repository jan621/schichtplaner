using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datastoring.EfCore.Migrations
{
    /// <inheritdoc />
    public partial class AddSortIndexToTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortIndex",
                table: "Tags",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortIndex",
                table: "Tags");
        }
    }
}
