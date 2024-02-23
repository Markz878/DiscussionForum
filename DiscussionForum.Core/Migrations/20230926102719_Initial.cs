using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionForum.Core.Migrations;

/// <inheritdoc />
public partial class Initial : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE USER [discussionforum-identity] FROM EXTERNAL PROVIDER;");
        migrationBuilder.Sql("ALTER ROLE db_datareader ADD MEMBER [discussionforum-identity];");
        migrationBuilder.Sql("ALTER ROLE db_datawriter ADD MEMBER [discussionforum-identity];");
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Topics",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastMessageTimeStamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Topics", x => x.Id);
                table.ForeignKey(
                    name: "FK_Topics_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Messages",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                EditedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                TopicId = table.Column<long>(type: "bigint", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Messages", x => x.Id);
                table.ForeignKey(
                    name: "FK_Messages_Topics_TopicId",
                    column: x => x.TopicId,
                    principalTable: "Topics",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Messages_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "MessageAttachedFiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                MessageId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MessageAttachedFiles", x => x.Id);
                table.ForeignKey(
                    name: "FK_MessageAttachedFiles_Messages_MessageId",
                    column: x => x.MessageId,
                    principalTable: "Messages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "MessageLikes",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                MessageId = table.Column<long>(type: "bigint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MessageLikes", x => new { x.MessageId, x.UserId });
                table.ForeignKey(
                    name: "FK_MessageLikes_Messages_MessageId",
                    column: x => x.MessageId,
                    principalTable: "Messages",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_MessageAttachedFiles_Id_Name",
            table: "MessageAttachedFiles",
            columns: new[] { "Id", "Name" },
            unique: true)
            .Annotation("SqlServer:Online", true);

        migrationBuilder.CreateIndex(
            name: "IX_MessageAttachedFiles_MessageId",
            table: "MessageAttachedFiles",
            column: "MessageId");

        migrationBuilder.CreateIndex(
            name: "IX_Messages_TopicId",
            table: "Messages",
            column: "TopicId");

        migrationBuilder.CreateIndex(
            name: "IX_Messages_UserId",
            table: "Messages",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Topics_LastMessageTimeStamp",
            table: "Topics",
            column: "LastMessageTimeStamp")
            .Annotation("SqlServer:Online", true);

        migrationBuilder.CreateIndex(
            name: "IX_Topics_UserId",
            table: "Topics",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true)
            .Annotation("SqlServer:Online", true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_UserName",
            table: "Users",
            column: "UserName",
            unique: true)
            .Annotation("SqlServer:Online", true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MessageAttachedFiles");

        migrationBuilder.DropTable(
            name: "MessageLikes");

        migrationBuilder.DropTable(
            name: "Messages");

        migrationBuilder.DropTable(
            name: "Topics");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
