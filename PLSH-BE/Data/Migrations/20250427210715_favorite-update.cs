using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class favoriteupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_favorites_Accounts_AccountId",
                table: "favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_favorites_Borrowers_BorrowerId",
                table: "favorites");

            migrationBuilder.DropIndex(
                name: "IX_favorites_AccountId",
                table: "favorites");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "favorites");

            migrationBuilder.AddForeignKey(
                name: "FK_favorites_Accounts_BorrowerId",
                table: "favorites",
                column: "BorrowerId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_favorites_Accounts_BorrowerId",
                table: "favorites");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "favorites",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_favorites_AccountId",
                table: "favorites",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_favorites_Accounts_AccountId",
                table: "favorites",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_favorites_Borrowers_BorrowerId",
                table: "favorites",
                column: "BorrowerId",
                principalTable: "Borrowers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
