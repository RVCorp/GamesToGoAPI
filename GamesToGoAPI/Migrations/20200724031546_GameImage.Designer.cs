﻿// <auto-generated />
using GamesToGoAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GamesToGoAPI.Migrations
{
    [DbContext(typeof(GamesToGoContext))]
    [Migration("20200724031546_GameImage")]
    partial class GameImage
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("GamesToGoAPI.Models.AnswerReport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int(11)");

                    b.Property<int>("AdminId")
                        .HasColumnName("adminID")
                        .HasColumnType("int(11)");

                    b.Property<int>("AnswertypeId")
                        .HasColumnName("answertypeID")
                        .HasColumnType("int(11)");

                    b.Property<string>("Details")
                        .IsRequired()
                        .HasColumnName("details")
                        .HasColumnType("varchar(100)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.Property<int>("ReportId")
                        .HasColumnName("reportID")
                        .HasColumnType("int(11)");

                    b.HasKey("Id");

                    b.HasIndex("AdminId")
                        .HasName("adminID_idx");

                    b.HasIndex("AnswertypeId")
                        .HasName("answertypeID_idx");

                    b.HasIndex("ReportId")
                        .HasName("reportID_idx");

                    b.ToTable("AnswerReport");
                });

            modelBuilder.Entity("GamesToGoAPI.Models.AnswerType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int(11)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("varchar(6)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.HasKey("Id");

                    b.ToTable("AnswerType");
                });

            modelBuilder.Entity("GamesToGoAPI.Models.Game", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int(11)");

                    b.Property<int>("CreatorId")
                        .HasColumnName("creatorID")
                        .HasColumnType("int(11)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnName("description")
                        .HasColumnType("varchar(150)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.Property<string>("Hash")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Image")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Maxplayers")
                        .HasColumnName("maxplayers")
                        .HasColumnType("int(11)");

                    b.Property<int>("Minplayers")
                        .HasColumnName("minplayers")
                        .HasColumnType("int(11)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("varchar(60)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId")
                        .HasName("creatorID_idx");

                    b.HasIndex("Id")
                        .IsUnique()
                        .HasName("id_UNIQUE");

                    b.ToTable("Game");
                });

            modelBuilder.Entity("GamesToGoAPI.Models.Report", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int(11)");

                    b.Property<int>("GameId")
                        .HasColumnName("gameID")
                        .HasColumnType("int(11)");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnName("reason")
                        .HasColumnType("varchar(100)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.Property<int>("UserId")
                        .HasColumnName("userID")
                        .HasColumnType("int(11)");

                    b.HasKey("Id");

                    b.HasIndex("GameId")
                        .HasName("gameID_idx");

                    b.HasIndex("UserId")
                        .HasName("userID_idx");

                    b.ToTable("Report");
                });

            modelBuilder.Entity("GamesToGoAPI.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int(11)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email")
                        .HasColumnType("varchar(100)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.Property<string>("Image")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnName("password")
                        .HasColumnType("char(128)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnName("username")
                        .HasColumnType("varchar(20)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.Property<int>("UsertypeId")
                        .HasColumnName("usertypeID")
                        .HasColumnType("int(11)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasName("email_UNIQUE");

                    b.HasIndex("Id")
                        .IsUnique()
                        .HasName("id_UNIQUE");

                    b.HasIndex("Username")
                        .IsUnique()
                        .HasName("username_UNIQUE");

                    b.HasIndex("UsertypeId")
                        .HasName("id_idx");

                    b.ToTable("User");
                });

            modelBuilder.Entity("GamesToGoAPI.Models.UserType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("int(11)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("varchar(5)")
                        .HasAnnotation("MySql:CharSet", "utf8mb4")
                        .HasAnnotation("MySql:Collation", "utf8mb4_0900_ai_ci");

                    b.HasKey("Id");

                    b.ToTable("UserType");
                });

            modelBuilder.Entity("GamesToGoAPI.Models.AnswerReport", b =>
                {
                    b.HasOne("GamesToGoAPI.Models.User", "Admin")
                        .WithMany("AnswerReport")
                        .HasForeignKey("AdminId")
                        .HasConstraintName("adminID")
                        .IsRequired();

                    b.HasOne("GamesToGoAPI.Models.AnswerType", "Answertype")
                        .WithMany("AnswerReport")
                        .HasForeignKey("AnswertypeId")
                        .HasConstraintName("answertypeID")
                        .IsRequired();

                    b.HasOne("GamesToGoAPI.Models.Report", "Report")
                        .WithMany("AnswerReport")
                        .HasForeignKey("ReportId")
                        .HasConstraintName("reportID")
                        .IsRequired();
                });

            modelBuilder.Entity("GamesToGoAPI.Models.Game", b =>
                {
                    b.HasOne("GamesToGoAPI.Models.User", "Creator")
                        .WithMany("Game")
                        .HasForeignKey("CreatorId")
                        .HasConstraintName("creatorID")
                        .IsRequired();
                });

            modelBuilder.Entity("GamesToGoAPI.Models.Report", b =>
                {
                    b.HasOne("GamesToGoAPI.Models.Game", "Game")
                        .WithMany("Report")
                        .HasForeignKey("GameId")
                        .HasConstraintName("gameID")
                        .IsRequired();

                    b.HasOne("GamesToGoAPI.Models.User", "User")
                        .WithMany("Report")
                        .HasForeignKey("UserId")
                        .HasConstraintName("userID")
                        .IsRequired();
                });

            modelBuilder.Entity("GamesToGoAPI.Models.User", b =>
                {
                    b.HasOne("GamesToGoAPI.Models.UserType", "Usertype")
                        .WithMany("User")
                        .HasForeignKey("UsertypeId")
                        .HasConstraintName("usertypeID")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
