using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class Shelf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Shelves_RoomId",
                table: "Shelves",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shelves_LibraryRooms_RoomId",
                table: "Shelves",
                column: "RoomId",
                principalTable: "LibraryRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shelves_LibraryRooms_RoomId",
                table: "Shelves");

            migrationBuilder.DropIndex(
                name: "IX_Shelves_RoomId",
                table: "Shelves");
        }
    }
}
