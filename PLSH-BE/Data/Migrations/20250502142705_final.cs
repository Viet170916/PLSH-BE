using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Fines",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_BookBorrowingId",
                table: "Fines",
                column: "BookBorrowingId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_BorrowerId",
                table: "Fines",
                column: "BorrowerId");

            migrationBuilder.CreateIndex(
                name: "IX_Fines_TransactionId",
                table: "Fines",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_Accounts_BorrowerId",
                table: "Fines",
                column: "BorrowerId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_BookBorrowings_BookBorrowingId",
                table: "Fines",
                column: "BookBorrowingId",
                principalTable: "BookBorrowings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Fines_Transactions_TransactionId",
                table: "Fines",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Fines_Accounts_BorrowerId",
                table: "Fines");

            migrationBuilder.DropForeignKey(
                name: "FK_Fines_BookBorrowings_BookBorrowingId",
                table: "Fines");

            migrationBuilder.DropForeignKey(
                name: "FK_Fines_Transactions_TransactionId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_Fines_BookBorrowingId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_Fines_BorrowerId",
                table: "Fines");

            migrationBuilder.DropIndex(
                name: "IX_Fines_TransactionId",
                table: "Fines");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Fines",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
