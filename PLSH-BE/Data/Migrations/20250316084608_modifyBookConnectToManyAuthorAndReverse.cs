using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class modifyBookConnectToManyAuthorAndReverse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Books");

            migrationBuilder.AddColumn<long>(
                name: "BookAuthorId",
                table: "Books",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "BookAuthorId",
                table: "Authors",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthorBook",
                columns: table => new
                {
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorBook", x => new { x.AuthorId, x.BookId });
                    table.ForeignKey(
                        name: "FK_AuthorBook_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorBook_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorBook_BookId",
                table: "AuthorBook",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthorBook");

            migrationBuilder.DropColumn(
                name: "BookAuthorId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "BookAuthorId",
                table: "Authors");

            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
