using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class audioBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "AudioBooks",
                newName: "Chapter");

            migrationBuilder.AddColumn<int>(
                name: "AudioResourceId",
                table: "AudioBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "AudioBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Voice",
                table: "AudioBooks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AudioBooks_AudioResourceId",
                table: "AudioBooks",
                column: "AudioResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AudioBooks_Resources_AudioResourceId",
                table: "AudioBooks",
                column: "AudioResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AudioBooks_Resources_AudioResourceId",
                table: "AudioBooks");

            migrationBuilder.DropIndex(
                name: "IX_AudioBooks_AudioResourceId",
                table: "AudioBooks");

            migrationBuilder.DropColumn(
                name: "AudioResourceId",
                table: "AudioBooks");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "AudioBooks");

            migrationBuilder.DropColumn(
                name: "Voice",
                table: "AudioBooks");

            migrationBuilder.RenameColumn(
                name: "Chapter",
                table: "AudioBooks",
                newName: "AccountId");
        }
    }
}
