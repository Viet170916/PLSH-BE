using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAddEpubToBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EpubResourceId",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Books_EpubResourceId",
                table: "Books",
                column: "EpubResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Resources_EpubResourceId",
                table: "Books",
                column: "EpubResourceId",
                principalTable: "Resources",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Resources_EpubResourceId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_EpubResourceId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "EpubResourceId",
                table: "Books");
        }
    }
}
