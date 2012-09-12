using System.Data.Entity;

namespace K2Calendar.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<UserInfoModel> Users { get; set; }
        public DbSet<RankModel> Ranks { get; set; }
        public DbSet<PostModel> Posts { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<CommentModel> Comments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostModel>()
            .HasMany(p => p.Categories).WithMany(c => c.Posts)
            .Map(t => t.MapLeftKey("PostId")
            .MapRightKey("CategoryId")
            .ToTable("PostCategories"));
        }
    }
}