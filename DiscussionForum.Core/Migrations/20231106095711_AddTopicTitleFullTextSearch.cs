using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionForum.Core.Migrations;

/// <inheritdoc />
public partial class AddTopicTitleFullTextSearch : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "MessageAttachedFiles",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);
        migrationBuilder.Sql(
            sql: "CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;",
            suppressTransaction: true);
        migrationBuilder.Sql(
            sql: "CREATE FULLTEXT INDEX ON Topics(Title) KEY INDEX PK_Topics;",
            suppressTransaction: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "MessageAttachedFiles",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(150)",
            oldMaxLength: 150);
        migrationBuilder.Sql(
            sql: "DROP FULLTEXT INDEX ON Topics;",
            suppressTransaction: true);
        migrationBuilder.Sql(
            sql: "DROP FULLTEXT CATALOG ftCatalog;",
            suppressTransaction: true);
    }
}
