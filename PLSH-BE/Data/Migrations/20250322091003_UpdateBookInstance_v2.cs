using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookInstance_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInstances_Books_BookId",
                table: "BookInstances");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "BookInstances",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BookIdRestore",
                table: "BookInstances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BookInstances",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "BookInstances",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookInstances_Books_BookId",
                table: "BookInstances",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookInstances_Books_BookId",
                table: "BookInstances");

            migrationBuilder.DropColumn(
                name: "BookIdRestore",
                table: "BookInstances");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BookInstances");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "BookInstances");

            migrationBuilder.AlterColumn<int>(
                name: "BookId",
                table: "BookInstances",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BookInstances_Books_BookId",
                table: "BookInstances",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
