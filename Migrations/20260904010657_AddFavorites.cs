using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace realworld_net.Migrations;

/// <inheritdoc />
public partial class AddFavorites : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "FavoritesCount",
            table: "Articles",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "Favorites",
            columns: table => new
            {
                UserId = table.Column<int>(type: "int", nullable: false),
                ArticleId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Favorites", x => new { x.UserId, x.ArticleId });
                table.ForeignKey(
                    name: "FK_Favorites_Articles_ArticleId",
                    column: x => x.ArticleId,
                    principalTable: "Articles",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Favorites_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Favorites_ArticleId",
            table: "Favorites",
            column: "ArticleId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Favorites");

        migrationBuilder.DropColumn(
            name: "FavoritesCount",
            table: "Articles");
    }
}
