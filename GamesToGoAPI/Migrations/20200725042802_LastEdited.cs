using Microsoft.EntityFrameworkCore.Migrations;

namespace GamesToGoAPI.Migrations
{
    public partial class LastEdited : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastEdited",
                table: "Game",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastEdited",
                table: "Game");
        }
    }
}
