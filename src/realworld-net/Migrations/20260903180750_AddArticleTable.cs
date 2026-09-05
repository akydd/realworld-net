using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace realworld_net.Migrations;

/// <inheritdoc />
public partial class AddArticleTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Articles",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                AuthorId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Articles", x => x.Id);
                table.ForeignKey(
                    name: "FK_Articles_Users_AuthorId",
                    column: x => x.AuthorId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Articles_AuthorId",
            table: "Articles",
            column: "AuthorId");

        migrationBuilder.CreateIndex(
            name: "IX_Articles_Slug",
            table: "Articles",
            column: "Slug",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Articles");
    }
}
