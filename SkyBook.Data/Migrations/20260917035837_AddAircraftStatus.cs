using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyBook.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Aircrafts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Aircrafts");
        }
    }
}
