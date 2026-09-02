using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace realworld_net.Migrations;

/// <inheritdoc />
public partial class UserPassword : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PasswordHash",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "PasswordSalt",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PasswordHash",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "PasswordSalt",
            table: "Users");
    }
}
