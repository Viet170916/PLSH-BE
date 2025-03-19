using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class EnableFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Authors",
                type: "VARCHAR(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            // migrationBuilder.CreateIndex(
            //     name: "idx_author_fullname",
            //     table: "Authors",
            //     column: "FullName")
            //     .Annotation("MySql:FullTextIndex", true);
            migrationBuilder.Sql(@"
        SET @index_exists = (SELECT COUNT(*) FROM information_schema.statistics 
                             WHERE table_schema = DATABASE() 
                             AND table_name = 'Authors' 
                             AND index_name = 'idx_author_fullname');
        SET @query = IF(@index_exists = 0, 'ALTER TABLE Authors ADD FULLTEXT INDEX idx_author_fullname (FullName)', 'SELECT 1');
        PREPARE stmt FROM @query;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.Sql(@"
            //     ALTER TABLE Authors DROP INDEX idx_author_fullname;
            //  ");
            migrationBuilder.DropIndex(
                name: "idx_author_fullname",
                table: "Authors");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Authors",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
