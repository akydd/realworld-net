using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace realworld_net.Migrations;

/// <inheritdoc />
public partial class RemoveUserPasswordSalt : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PasswordSalt",
            table: "Users");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PasswordSalt",
            table: "Users",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");
    }
}
