using Microsoft.EntityFrameworkCore.Migrations;

namespace GamesToGoAPI.Migrations
{
    public partial class GameImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Game",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Game");
        }
    }
}
