using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GamesToGo.API.Migrations
{
    public partial class DatabaseRework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "creatorID",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "gameID",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "userID",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "usertypeID",
                table: "User");

            migrationBuilder.DropTable(
                name: "AnswerReport");

            migrationBuilder.DropTable(
                name: "UserType");

            migrationBuilder.DropTable(
                name: "AnswerType");

            migrationBuilder.DropIndex(
                name: "email_UNIQUE",
                table: "User");

            migrationBuilder.DropIndex(
                name: "id_idx",
                table: "User");

            migrationBuilder.DropIndex(
                name: "id_UNIQUE",
                table: "User");

            migrationBuilder.DropIndex(
                name: "username_UNIQUE",
                table: "User");

            migrationBuilder.DropIndex(
                name: "id_UNIQUE",
                table: "Game");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "User",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "User",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "userID",
                table: "Report",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "reason",
                table: "Report",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "gameID",
                table: "Report",
                newName: "GameId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Report",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "userID_idx",
                table: "Report",
                newName: "IX_Report_UserId");

            migrationBuilder.RenameIndex(
                name: "gameID_idx",
                table: "Report",
                newName: "IX_Report_GameId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Game",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "minplayers",
                table: "Game",
                newName: "Minplayers");

            migrationBuilder.RenameColumn(
                name: "maxplayers",
                table: "Game",
                newName: "Maxplayers");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Game",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "creatorID",
                table: "Game",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Game",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "creatorID_idx",
                table: "Game",
                newName: "IX_Game_CreatorId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "User",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "User",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "LogoutTime",
                table: "User",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Report",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Report",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "Report",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Report",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "ReportTypeID",
                table: "Report",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeReported",
                table: "Report",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Game",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(60)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Minplayers",
                table: "Game",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AlterColumn<int>(
                name: "Maxplayers",
                table: "Game",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Game",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "CreatorId",
                table: "Game",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int(11)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Game",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int(11)")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateTable(
                name: "ReportType",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserLogin",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    Password = table.Column<string>(type: "longtext CHARACTER SET utf8mb4", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogin", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserLogin_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Report_ReportTypeID",
                table: "Report",
                column: "ReportTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogin_UserId",
                table: "UserLogin",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Game_User_CreatorId",
                table: "Game",
                column: "CreatorId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Report_Game_GameId",
                table: "Report",
                column: "GameId",
                principalTable: "Game",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Report_ReportType_ReportTypeID",
                table: "Report",
                column: "ReportTypeID",
                principalTable: "ReportType",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Report_User_UserId",
                table: "Report",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            migrationBuilder.Sql("START TRANSACTION;" +
                                 "INSERT INTO UserLogin (Email, Password, UserId) SELECT User.Email, User.Password, User.ID FROM User;" +
                                 "COMMIT;");

            migrationBuilder.DropColumn(
                name: "email",
                table: "User");

            migrationBuilder.DropColumn(
                name: "password",
                table: "User");

            migrationBuilder.DropColumn(
                name: "usertypeID",
                table: "User");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Game_User_CreatorId",
                table: "Game");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_Game_GameId",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_ReportType_ReportTypeID",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_User_UserId",
                table: "Report");

            migrationBuilder.DropTable(
                name: "ReportType");

            migrationBuilder.DropTable(
                name: "UserLogin");

            migrationBuilder.DropIndex(
                name: "IX_Report_ReportTypeID",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "LogoutTime",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ReportTypeID",
                table: "Report");

            migrationBuilder.DropColumn(
                name: "TimeReported",
                table: "Report");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "User",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "User",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Report",
                newName: "userID");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Report",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "Report",
                newName: "gameID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Report",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_Report_UserId",
                table: "Report",
                newName: "userID_idx");

            migrationBuilder.RenameIndex(
                name: "IX_Report_GameId",
                table: "Report",
                newName: "gameID_idx");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Game",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Minplayers",
                table: "Game",
                newName: "minplayers");

            migrationBuilder.RenameColumn(
                name: "Maxplayers",
                table: "Game",
                newName: "maxplayers");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Game",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Game",
                newName: "creatorID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Game",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_Game_CreatorId",
                table: "Game",
                newName: "creatorID_idx");

            migrationBuilder.AlterColumn<string>(
                name: "username",
                table: "User",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "User",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "User",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "User",
                type: "char(128)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "usertypeID",
                table: "User",
                type: "int(11)",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "userID",
                table: "Report",
                type: "int(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "reason",
                table: "Report",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "gameID",
                table: "Report",
                type: "int(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "Report",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Game",
                type: "varchar(60)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "minplayers",
                table: "Game",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "maxplayers",
                table: "Game",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Game",
                type: "varchar(150)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext CHARACTER SET utf8mb4",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "creatorID",
                table: "Game",
                type: "int(11)",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "Game",
                type: "int(11)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.CreateTable(
                name: "AnswerType",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(6)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UserType",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(5)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserType", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AnswerReport",
                columns: table => new
                {
                    id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    adminID = table.Column<int>(type: "int(11)", nullable: false),
                    answertypeID = table.Column<int>(type: "int(11)", nullable: false),
                    details = table.Column<string>(type: "varchar(100)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reportID = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerReport", x => x.id);
                    table.ForeignKey(
                        name: "adminID",
                        column: x => x.adminID,
                        principalTable: "User",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "answertypeID",
                        column: x => x.answertypeID,
                        principalTable: "AnswerType",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "reportID",
                        column: x => x.reportID,
                        principalTable: "Report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "email_UNIQUE",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "id_idx",
                table: "User",
                column: "usertypeID");

            migrationBuilder.CreateIndex(
                name: "id_UNIQUE",
                table: "User",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "username_UNIQUE",
                table: "User",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "id_UNIQUE",
                table: "Game",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "adminID_idx",
                table: "AnswerReport",
                column: "adminID");

            migrationBuilder.CreateIndex(
                name: "answertypeID_idx",
                table: "AnswerReport",
                column: "answertypeID");

            migrationBuilder.CreateIndex(
                name: "reportID_idx",
                table: "AnswerReport",
                column: "reportID");

            migrationBuilder.AddForeignKey(
                name: "creatorID",
                table: "Game",
                column: "creatorID",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "gameID",
                table: "Report",
                column: "gameID",
                principalTable: "Game",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "userID",
                table: "Report",
                column: "userID",
                principalTable: "User",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "usertypeID",
                table: "User",
                column: "usertypeID",
                principalTable: "UserType",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
