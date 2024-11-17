using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionForum.Core.Migrations;

/// <inheritdoc />
public partial class RoleAssignments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE USER [discussionforum-identity] FROM EXTERNAL PROVIDER;
            ALTER ROLE db_datareader ADD MEMBER [discussionforum-identity];
            ALTER ROLE db_datawriter ADD MEMBER [discussionforum-identity];
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

    }
}
