using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace GamesToGoAPI.Models
{
    public partial class GamesToGoContext : DbContext
    {
        public GamesToGoContext()
        {
        }

        public GamesToGoContext(DbContextOptions<GamesToGoContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AnswerReport> AnswerReport { get; set; }
        public virtual DbSet<AnswerType> AnswerType { get; set; }
        public virtual DbSet<Game> Game { get; set; }
        public virtual DbSet<Report> Report { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserType> UserType { get; set; }
 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnswerReport>(entity =>
            {
                entity.HasIndex(e => e.AdminId)
                    .HasName("adminID_idx");

                entity.HasIndex(e => e.AnswertypeId)
                    .HasName("answertypeID_idx");

                entity.HasIndex(e => e.ReportId)
                    .HasName("reportID_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AdminId)
                    .HasColumnName("adminID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.AnswertypeId)
                    .HasColumnName("answertypeID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Details)
                    .IsRequired()
                    .HasColumnName("details")
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.ReportId)
                    .HasColumnName("reportID")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Admin)
                    .WithMany(p => p.AnswerReport)
                    .HasForeignKey(d => d.AdminId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("adminID");

                entity.HasOne(d => d.Answertype)
                    .WithMany(p => p.AnswerReport)
                    .HasForeignKey(d => d.AnswertypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("answertypeID");

                entity.HasOne(d => d.Report)
                    .WithMany(p => p.AnswerReport)
                    .HasForeignKey(d => d.ReportId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("reportID");
            });

            modelBuilder.Entity<AnswerType>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(6)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasIndex(e => e.CreatorId)
                    .HasName("creatorID_idx");

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CreatorId)
                    .HasColumnName("creatorID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasColumnName("description")
                    .HasColumnType("varchar(150)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Maxplayers)
                    .HasColumnName("maxplayers")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Minplayers)
                    .HasColumnName("minplayers")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(60)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.HasOne(d => d.Creator)
                    .WithMany(p => p.Game)
                    .HasForeignKey(d => d.CreatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("creatorID");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasIndex(e => e.GameId)
                    .HasName("gameID_idx");

                entity.HasIndex(e => e.UserId)
                    .HasName("userID_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.GameId)
                    .HasColumnName("gameID")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasColumnName("reason")
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UserId)
                    .HasColumnName("userID")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Game)
                    .WithMany(p => p.Report)
                    .HasForeignKey(d => d.GameId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("gameID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Report)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("userID");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email)
                    .HasName("email_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Id)
                    .HasName("id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Username)
                    .HasName("username_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.UsertypeId)
                    .HasName("id_idx");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnName("email")
                    .HasColumnType("varchar(100)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasColumnType("char(128)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasColumnType("varchar(20)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");

                entity.Property(e => e.UsertypeId)
                    .HasColumnName("usertypeID")
                    .HasColumnType("int(11)");

                entity.HasOne(d => d.Usertype)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.UsertypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("usertypeID");
            });

            modelBuilder.Entity<UserType>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasColumnType("varchar(5)")
                    .HasCharSet("utf8mb4")
                    .HasCollation("utf8mb4_0900_ai_ci");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
