using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RowShelfId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInstances_RowShelves_ShelfId",
                table: "BookInstances");

            migrationBuilder.DropIndex(
                name: "IX_BookInstances_ShelfId",
                table: "BookInstances");

            migrationBuilder.DropColumn(
                name: "ShelfId",
                table: "BookInstances");

            migrationBuilder.AlterColumn<long>(
                name: "RowShelfId",
                table: "BookInstances",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Position",
                table: "BookInstances",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_BookInstances_RowShelfId",
                table: "BookInstances",
                column: "RowShelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookInstances_RowShelves_RowShelfId",
                table: "BookInstances",
                column: "RowShelfId",
                principalTable: "RowShelves",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInstances_RowShelves_RowShelfId",
                table: "BookInstances");

            migrationBuilder.DropIndex(
                name: "IX_BookInstances_RowShelfId",
                table: "BookInstances");

            migrationBuilder.AlterColumn<int>(
                name: "RowShelfId",
                table: "BookInstances",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Position",
                table: "BookInstances",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShelfId",
                table: "BookInstances",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookInstances_ShelfId",
                table: "BookInstances",
                column: "ShelfId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookInstances_RowShelves_ShelfId",
                table: "BookInstances",
                column: "ShelfId",
                principalTable: "RowShelves",
                principalColumn: "Id");
        }
    }
}
