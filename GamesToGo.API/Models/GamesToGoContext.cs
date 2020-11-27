using Microsoft.EntityFrameworkCore;
// ReSharper disable StringLiteralTypo

namespace GamesToGo.API.Models
{
    public class GamesToGoContext : DbContext
    {
        public GamesToGoContext()
        {
        }

        public GamesToGoContext(DbContextOptions<GamesToGoContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Game> Game { get; set; }
        public virtual DbSet<Report> Report { get; set; }
        public virtual DbSet<User> User { get; set; }
        
        public virtual DbSet<UserLogin> UserLogin { get; set; }
        
        public virtual DbSet<ReportType> ReportType { get; set; }
        
        public virtual DbSet<UserStatistic> UserStatistic { get; set; }
    }
}
