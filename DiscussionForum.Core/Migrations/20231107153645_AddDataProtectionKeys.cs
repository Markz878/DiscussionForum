﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionForum.Core.Migrations;

/// <inheritdoc />
public partial class AddDataProtectionKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DataProtectionKeys",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                FriendlyName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                Xml = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_DataProtectionKeys", x => x.Id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DataProtectionKeys");
    }
}
