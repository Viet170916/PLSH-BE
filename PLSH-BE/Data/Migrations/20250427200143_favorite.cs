using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class favorite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Favorites",
                table: "Favorites");

            migrationBuilder.RenameTable(
                name: "Favorites",
                newName: "favorites");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "favorites",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_favorites",
                table: "favorites",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ShareLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    ShareUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnablePlatform = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ShortenUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareLinks", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_AccountId",
                table: "favorites",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_BookId",
                table: "favorites",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_BorrowerId",
                table: "favorites",
                column: "BorrowerId");

            migrationBuilder.AddForeignKey(
                name: "FK_favorites_Accounts_AccountId",
                table: "favorites",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_favorites_Books_BookId",
                table: "favorites",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_favorites_Borrowers_BorrowerId",
                table: "favorites",
                column: "BorrowerId",
                principalTable: "Borrowers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_favorites_Accounts_AccountId",
                table: "favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_favorites_Books_BookId",
                table: "favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_favorites_Borrowers_BorrowerId",
                table: "favorites");

            migrationBuilder.DropTable(
                name: "ShareLinks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_favorites",
                table: "favorites");

            migrationBuilder.DropIndex(
                name: "IX_favorites_AccountId",
                table: "favorites");

            migrationBuilder.DropIndex(
                name: "IX_favorites_BookId",
                table: "favorites");

            migrationBuilder.DropIndex(
                name: "IX_favorites_BorrowerId",
                table: "favorites");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "favorites");

            migrationBuilder.RenameTable(
                name: "favorites",
                newName: "Favorites");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Favorites",
                table: "Favorites",
                column: "Id");
        }
    }
}
