using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class shelfchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Angle",
                table: "Shelves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RootX",
                table: "Shelves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RootY",
                table: "Shelves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Shelves",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "X1",
                table: "Shelves",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "X2",
                table: "Shelves",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Y1",
                table: "Shelves",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Y2",
                table: "Shelves",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Angle",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "RootX",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "RootY",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "X1",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "X2",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "Y1",
                table: "Shelves");

            migrationBuilder.DropColumn(
                name: "Y2",
                table: "Shelves");
        }
    }
}
