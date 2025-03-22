using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInstance_Books_BookId",
                table: "BookInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_BookInstance_Bookshelves_BookShelfId",
                table: "BookInstance");

            migrationBuilder.DropTable(
                name: "Bookshelves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookInstance",
                table: "BookInstance");

            migrationBuilder.RenameTable(
                name: "BookInstance",
                newName: "BookInstances");

            migrationBuilder.RenameColumn(
                name: "BookShelfId",
                table: "BookInstances",
                newName: "ShelfId");

            migrationBuilder.RenameColumn(
                name: "BookOnRowShelfId",
                table: "BookInstances",
                newName: "RowShelfId");

            migrationBuilder.RenameIndex(
                name: "IX_BookInstance_BookShelfId",
                table: "BookInstances",
                newName: "IX_BookInstances_ShelfId");

            migrationBuilder.RenameIndex(
                name: "IX_BookInstance_BookId",
                table: "BookInstances",
                newName: "IX_BookInstances_BookId");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "BookInstances",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "BookInstances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookInstances",
                table: "BookInstances",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RowShelves_ShelfId",
                table: "RowShelves",
                column: "ShelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookInstances_Books_BookId",
                table: "BookInstances",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookInstances_RowShelves_ShelfId",
                table: "BookInstances",
                column: "ShelfId",
                principalTable: "RowShelves",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RowShelves_Shelves_ShelfId",
                table: "RowShelves",
                column: "ShelfId",
                principalTable: "Shelves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInstances_Books_BookId",
                table: "BookInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_BookInstances_RowShelves_ShelfId",
                table: "BookInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_RowShelves_Shelves_ShelfId",
                table: "RowShelves");

            migrationBuilder.DropIndex(
                name: "IX_RowShelves_ShelfId",
                table: "RowShelves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BookInstances",
                table: "BookInstances");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "BookInstances");

            migrationBuilder.RenameTable(
                name: "BookInstances",
                newName: "BookInstance");

            migrationBuilder.RenameColumn(
                name: "ShelfId",
                table: "BookInstance",
                newName: "BookShelfId");

            migrationBuilder.RenameColumn(
                name: "RowShelfId",
                table: "BookInstance",
                newName: "BookOnRowShelfId");

            migrationBuilder.RenameIndex(
                name: "IX_BookInstances_ShelfId",
                table: "BookInstance",
                newName: "IX_BookInstance_BookShelfId");

            migrationBuilder.RenameIndex(
                name: "IX_BookInstances_BookId",
                table: "BookInstance",
                newName: "IX_BookInstance_BookId");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "BookInstance",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookInstance",
                table: "BookInstance",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Bookshelves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BookId = table.Column<long>(type: "bigint", nullable: true),
                    ColName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Position = table.Column<int>(type: "int", nullable: false),
                    RowShelfId = table.Column<long>(type: "bigint", nullable: false),
                    ShelfId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookshelves", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_BookInstance_Books_BookId",
                table: "BookInstance",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookInstance_Bookshelves_BookShelfId",
                table: "BookInstance",
                column: "BookShelfId",
                principalTable: "Bookshelves",
                principalColumn: "Id");
        }
    }
}
