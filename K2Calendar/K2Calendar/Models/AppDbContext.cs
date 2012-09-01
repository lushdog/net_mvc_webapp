using System.Data.Entity;

namespace K2Calendar.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserInfoModel> Users { get; set; }
        public DbSet<UserRankModel> Ranks { get; set; }
    }
}